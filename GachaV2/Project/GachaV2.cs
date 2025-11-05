using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Unity.Services.CloudCode.Apis;
using Unity.Services.CloudCode.Core;
using Unity.Services.CloudCode.Shared;
using Unity.Services.CloudSave.Model;
using Unity.Services.Economy.Model;

namespace CheerUpMyHero.CloudCode
{
    public enum Rarity
    {
        Epic,
        Rare,
        Common
    }

    public class RarityInfo
    {
        public Rarity RarityType { get; set; }
        public int Weight { get; set; }
        public List<int> IDs { get; set; } = new List<int>();
    }

    public class ResultUnit
    {
        // JsonProperty를 사용하면 클라이언트가 받는 JSON 키 이름을 커스터마이징할 수 있습니다.
        [JsonProperty("unit_id")]
        public int UnitId { get; set; }

        [JsonProperty("rarity")]
        [JsonConverter(typeof(StringEnumConverter))] // Enum을 "Epic" 같은 문자열로 변환
        public Rarity Rarity { get; set; }

    }

    // --- 클라이언트에 반환할 데이터 구조 ---
    public class GachaResult
    {
        [JsonProperty("result_unit")]
        public List<ResultUnit> ResultUnit { get; set; } = new();

        [JsonProperty("current_pity_count")]
        public int CurrentPityCount { get; set; }

        [JsonProperty("user_currency")]
        public int UserCurrency { get; set; }
    }

    public class GachaBannerConfig
    {
        [JsonProperty("pityThreshold")]
        public int PityThreshold { get; set; }

        [JsonProperty("guaranteedItemId")]
        public int GuaranteedItemId { get; set; }

        [JsonProperty("rarityTable")]
        public List<RarityInfo> RarityTable { get; set; } = new();

        public string BannerId { get; set; } = string.Empty;
    }

    public class PlayerPityData
    {
        // Key: bannerId ("normal", "pickup"), Value: pity count
        public Dictionary<string, int> PityCounters { get; set; } = new Dictionary<string, int>();
    }

    // --- 메인 모듈 ---
    public class GachaModuleV2
    {
        // --- key 값 모음 ---
        private const string GACHA_PURCHASE_ID_SINGLE = "ONE_GACHA";        //Economy - virtual purchase 
        private const string GACHA_PURCHASE_ID_TEN = "TEN_GACHA";           //Economy - virtual purchase 
        private const string TICKET_ID = "TICKET";                          //Economy - currency
        private const string PITY_COUNT_KEY_PREFIX = "pityCount_";            //Cloud Save
        private const string GACHA_TABLE_CONFIG_KEY = "GACHA_BANNERS";      //Remote Config

        // ★★★ 중요: Random 인스턴스는 static으로 선언하여 시드 값 문제를 방지해야 합니다.
        private static readonly Random s_rand = new Random();

        // 확률표를 static 변수로 만들어 캐싱합니다.
        private static Dictionary<string, GachaBannerConfig> s_gachaConfig = new();
        private static readonly object s_configLock = new object(); // 동시성 문제를 방지하기 위한 lock 객체


        // --- 1회 뽑기 함수 ---
        [CloudCodeFunction("DrawGachaOne")]
        public async Task<GachaResult> DrawGachaItem(IExecutionContext context, IGameApiClient gameApiClient, string bannerId)
        {
            // 10회 뽑기 함수를 재활용하여 1회 뽑기를 구현합니다.
            return await PerformGachaDraw(context, gameApiClient, bannerId, 1);
        }

        // --- 10회 뽑기 함수  ---
        [CloudCodeFunction("DrawGachaTen")]
        public async Task<GachaResult> DrawGachaItemTen(IExecutionContext context, IGameApiClient gameApiClient, string bannerId)
        {
            return await PerformGachaDraw(context, gameApiClient, bannerId, 10);
        }

        // --- 실제 가챠 로직을 수행하는 공통 함수 ---
        private async Task<GachaResult> PerformGachaDraw(IExecutionContext context, IGameApiClient gameApiClient, string bannerId, int drawCount)
        {
            // --- 추가된 부분 ---
            // PlayerId가 없으면 가챠를 진행할 수 없으므로, 명확한 오류를 발생시키고 함수를 중단합니다.
            if (string.IsNullOrEmpty(context.PlayerId))
            {
                throw new InvalidOperationException("Player ID is not available in the current context. This function must be called by a player.");
            }
            // 이 검사를 통과하면, 컴파일러는 이 아래부터 context.PlayerId가 절대 null이 아님을 인지합니다.
            // 따라서 더 이상 경고가 발생하지 않습니다.


            // 1. 전체 가챠 설정을 Remote Config에서 불러와 캐싱
            await InitializeGachaConfigAsync(context, gameApiClient);

            // 2. 요청된 bannerId에 해당하는 설정을 가져오기
            if (!s_gachaConfig.TryGetValue(bannerId, out var bannerConfig))
            {
                throw new Exception($"Invalid bannerId: {bannerId}");
            }

            // 3. bannerId를 사용하여 동적인 Cloud Save 키 생성
            string pityKey = PITY_COUNT_KEY_PREFIX + bannerId;
            int currentPityCount = await LoadPityCount(context, gameApiClient, pityKey);

            // 2. 재화 차감 (Economy Virtual Purchase)
            string purchaseId = drawCount == 1 ? GACHA_PURCHASE_ID_SINGLE : GACHA_PURCHASE_ID_TEN;
            var purchaseRequest = new PlayerPurchaseVirtualRequest(purchaseId);
            await gameApiClient.EconomyPurchases.MakeVirtualPurchaseAsync(context, context.AccessToken, context.ProjectId, context.PlayerId, purchaseRequest);

            List<ResultUnit> rewardedUnits = new();
            int pityCountForLoop = currentPityCount;

            // 3. 뽑기 횟수만큼 반복
            for (int i = 0; i < drawCount; i++)
            {
                pityCountForLoop++;

                RarityInfo selectedRarity;
                int selectedItemId;

                if (pityCountForLoop >= bannerConfig.PityThreshold)
                {
                    // 천장 도달!
                    selectedItemId = bannerConfig.GuaranteedItemId;

                    // 천장 아이템의 등급을 찾아야 IsHighestTierItem 로직이 올바르게 동작합니다.
                    selectedRarity = bannerConfig.RarityTable.First(r => r.IDs.Contains(selectedItemId));
                }
                else
                {
                    // 일반 확률 뽑기
                    selectedRarity = SelectRarity(bannerConfig.RarityTable);
                    selectedItemId = SelectItemId(selectedRarity);
                }

                rewardedUnits.Add(new ResultUnit { UnitId = selectedItemId, Rarity = selectedRarity.RarityType });

                // 천장 도달 또는 중간에 Epic 등급 획득 시 카운트 초기화
                if (pityCountForLoop >= bannerConfig.PityThreshold || selectedRarity.RarityType == Rarity.Epic)
                {
                    pityCountForLoop = 0;
                }
            }

            // 4. 최종 티켓 잔액 조회
            int finalUserCurrency = 0;
            try
            {
                var balancesResponse = await gameApiClient.EconomyCurrencies.GetPlayerCurrenciesAsync(context, context.AccessToken, context.ProjectId, context.PlayerId);
                var ticketBalance = balancesResponse.Data.Results.FirstOrDefault(b => b.CurrencyId == TICKET_ID);

                if (ticketBalance != null)
                {
                    finalUserCurrency = Convert.ToInt32(ticketBalance.Balance);
                }
            }
            catch
            {
                // 재화 정보 조회를 실패하더라도 가챠의 핵심 결과는 전달되어야 하므로,
                // 에러를 던지지 않고 기본값(0)을 사용하거나 -1 같은 특정 값으로 표기할 수 있습니다.
                finalUserCurrency = -1; // 조회 실패를 의미
            }


            // 5. 최종 천장 카운트를 Cloud Save에 저장
            await gameApiClient.CloudSaveData.SetItemAsync(context, context.AccessToken, context.ProjectId, context.PlayerId, new SetItemBody(pityKey, pityCountForLoop));

            // 6. 결과 반환
            return new GachaResult
            {
                ResultUnit = rewardedUnits,
                CurrentPityCount = pityCountForLoop,
                UserCurrency = finalUserCurrency
            };
        }


        // ★ 변경점: Remote Config에서 확률표를 불러와 캐싱하는 초기화 함수
        private async Task InitializeGachaConfigAsync(IExecutionContext context, IGameApiClient gameApiClient)
        {
            // 이미 테이블이 초기화되었다면 아무것도 하지 않고 즉시 반환 (캐싱)
            if (s_gachaConfig != null)
            {
                return;
            }

            // 동시성 제어: 여러 요청이 동시에 들어와도 단 하나의 요청만 테이블을 초기화하도록 보장
            lock (s_configLock)
            {
                // lock 내부에서 한 번 더 확인 (Double-checked locking)
                if (s_gachaConfig != null)
                {
                    return;
                }
            }

            try
            {
                // Remote Config에서 설정값 가져오기
                var response = await gameApiClient.RemoteConfigSettings.AssignSettingsGetAsync(context, context.AccessToken, context.ProjectId, context.EnvironmentId, key: new List<string> { GACHA_TABLE_CONFIG_KEY });

                var config = response.Data.Configs.Settings;

                if (config == null)
                {
                    throw new Exception($"Remote Controll == null. 혹시 KEY가 잘못되었나요?");
                }




                lock (s_configLock)
                {
                    s_gachaConfig = ConvertToGachaBannerConfigs(config);
                }

                
            }
            catch (Exception ex)
            {
                // 초기화 실패 시 로깅하고 예외를 다시 던져서 가챠 실행을 중단시킴
                // logger.LogError(ex, "Failed to initialize gacha table from Remote Config.");
                throw new Exception("Gacha system is currently unavailable. Failed to load configuration.", ex);
            }
        }


        // --- 기존의 확률 계산 헬퍼 함수들 ---
        private RarityInfo SelectRarity(List<RarityInfo> table)
        {
            // table이 null인 경우는 InitializeGachaTableAsync에서 예외를 던지므로 여기서는 null이 아님을 가정할 수 있음
            int totalWeight = table.Sum(r => r.Weight);
            double randomValue = s_rand.NextDouble() * totalWeight;

            double cumulativeWeight = 0;
            foreach (var rarityInfo in table)
            {
                cumulativeWeight += rarityInfo.Weight;
                if (randomValue < cumulativeWeight)
                {
                    return rarityInfo;
                }
            }
            return table.Last();
        }

        private int SelectItemId(RarityInfo selectedRarity)
        {
            int index = s_rand.Next(0, selectedRarity.IDs.Count);
            return selectedRarity.IDs[index];
        }

        // Cloud Save에서 천장 카운트를 불러오는 헬퍼 함수
        private async Task<int> LoadPityCount(IExecutionContext context, IGameApiClient gameApiClient, string pityKey)
        {
            try
            {
                // --- 추가된 부분 ---
                // PlayerId가 없으면 가챠를 진행할 수 없으므로, 명확한 오류를 발생시키고 함수를 중단합니다.
                if (string.IsNullOrEmpty(context.PlayerId))
                {
                    throw new InvalidOperationException("Player ID is not available in the current context. This function must be called by a player.");
                }
                // 이 검사를 통과하면, 컴파일러는 이 아래부터 context.PlayerId가 절대 null이 아님을 인지합니다.
                // 따라서 더 이상 경고가 발생하지 않습니다.

                var response = await gameApiClient.CloudSaveData.GetItemsAsync(context, context.AccessToken, context.ProjectId, context.PlayerId, new List<string> { pityKey });
                if (response.Data.Results.Any())
                {
                    return Convert.ToInt32(response.Data.Results[0].Value);
                }
            }
            catch (ApiException e) when (((int)e.Response.StatusCode) == 404)
            {
                return 0; // 데이터가 없는 정상적인 경우
            }
            return 0; // 예외 발생 시 안전하게 0으로 처리
        }

        private Dictionary<string, GachaBannerConfig> ConvertToGachaBannerConfigs(Dictionary<string, object> remoteConfigData)
        {
            var gachaBanners = new Dictionary<string, GachaBannerConfig>();

            // remoteConfigData 자체에 "gachaBanners" 키가 있는지 확인하는 것이 더 안전할 수 있습니다.
            // 여기서는 기존 로직을 유지하겠습니다.
            if (remoteConfigData.TryGetValue("gachaBanners", out var bannersObject) && bannersObject is Dictionary<string, object> bannersDict)
            {
                foreach (var entry in bannersDict)
                {
                    try
                    {
                        // 1. entry.Value를 다시 JSON 문자열로 변환
                        string jsonString = JsonConvert.SerializeObject(entry.Value);

                        // 2. Deserialize 실행. 결과는 null일 수 있으므로 nullable 타입 변수(GachaBannerConfig?)에 받습니다.
                        GachaBannerConfig? bannerConfig = JsonConvert.DeserializeObject<GachaBannerConfig>(jsonString);

                        // --- 추가된 부분 ---
                        // 3. 결과가 null인지 확인합니다.
                        if (bannerConfig == null)
                        {
                            // Deserialization 실패. 로그를 남기고 이 배너는 건너뛰거나, 전체를 실패 처리할 수 있습니다.
                            // 여기서는 로그를 남기고 건너뛰는 방식을 선택합니다.
                            Console.WriteLine($"Failed to deserialize banner config for key '{entry.Key}'. The JSON data might be invalid or null.");
                            continue; // 다음 배너로 넘어감
                        }

                        // 이 if 문을 통과하면, 컴파일러는 bannerConfig가 더 이상 null이 아님을 인지합니다.
                        bannerConfig.BannerId = entry.Key; // BannerId 설정 추가
                        gachaBanners.Add(entry.Key, bannerConfig);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error converting banner config for key '{entry.Key}': {ex.Message}");
                    }
                }
            }

            return gachaBanners;
        }
    }



    //
    public class ModuleConfig : ICloudCodeSetup
    {
        public void Setup(ICloudCodeConfig config)
        {
            config.Dependencies.AddSingleton(GameApiClient.Create()); //서버에서는 싱글톤을 이렇게 사용
        }
    }
};




