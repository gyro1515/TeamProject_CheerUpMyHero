using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
        public int UnitId { get; set; }

        // Enum을 "Epic" 같은 문자열로 변환
        public Rarity Rarity { get; set; }

    }

    // --- 클라이언트에 반환할 데이터 구조 ---
    public class GachaResult
    {
        public List<ResultUnit> ResultUnit { get; set; } = new();

        public int CurrentPityCount { get; set; }

        public int UserCurrency { get; set; }
    }

    public class GachaBannerConfig
    {
        public int PityThreshold { get; set; }

        public int GuaranteedItemId { get; set; }

        public List<RarityInfo> RarityTable { get; set; } = new();

        public string BannerId { get; set; } = string.Empty;
    }

    public class PlayerPityData
    {
        // Key: bannerId ("normal", "pickup"), Value: pity count
        public Dictionary<string, int> PityCounters { get; set; } = new Dictionary<string, int>();
    }

    //클라이언트에게 전달할 배너 정보를 담을 DTO 클래스를 명확하게 정의
    public class GachaBannerClientInfo
    {
        public string BannerId { get; set; } = string.Empty;
        public int PityThreshold { get; set; }

        // 여기에 클라이언트가 UI를 그리는 데 필요한 추가 정보를 포함시킬 수 있습니다.
        // 예: [JsonProperty("displayName")] public string DisplayName { get; set; }
        // 예: [JsonProperty("costSingle")] public int CostSingle { get; set; }
    }

    // --- 메인 모듈 ---
    public class GachaModuleV2
    {
        // --- key 값 모음 ---
        private const string GACHA_PURCHASE_ID_SINGLE = "ONE_GACHA";        //Economy - virtual purchase 
        private const string GACHA_PURCHASE_ID_TEN = "TEN_GACHA";           //Economy - virtual purchase 
        private const string TICKET_ID = "TICKET";                          //Economy - currency
        private const string PITY_COUNT_KEY_PREFIX = "PITYCOUNT_";          //Cloud Save
        private const string GACHA_TABLE_CONFIG_KEY = "GACHA_BANNERS";      //Remote Config

        // ★★★ 중요: Random 인스턴스는 static으로 선언하여 시드 값 문제를 방지해야 합니다.
        private static readonly Random s_rand = new Random();

        // 확률표를 static 변수로 만들어 캐싱합니다.
        private static Dictionary<string, GachaBannerConfig> s_gachaConfig = new();
        private static readonly object s_configLock = new object(); // 동시성 문제를 방지하기 위한 lock 객체

        private readonly ILogger<GachaModuleV2> _logger;
        public GachaModuleV2(ILogger<GachaModuleV2> logger)
        {
            _logger = logger;
        }

        // 서버 깨우기
        [CloudCodeFunction("WakeUpServer")]
        public void WakeUpServer()
        {
            //그냥 비어두긴 좀 그러니까
            int i = 1;
            i++;
        }

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

        // 가챠 정보 불러오기
        [CloudCodeFunction("GetGachaBanners")]
        public async Task<List<GachaBannerClientInfo>> GetGachaBanners(IExecutionContext context, IGameApiClient gameApiClient)
        {
 
            // 1. 서버의 GACHA_CONFIG를 불러옵니다. (기존 로직 재사용)
            await InitializeGachaConfigAsync(context, gameApiClient);

            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto
            };

            string json = JsonConvert.SerializeObject(s_gachaConfig, Formatting.Indented, settings);

            // 클라이언트에 필요한 정보만 담은 GachaBannerClientInfo 객체를 생성하여 반환
            var clientBanners = s_gachaConfig.Select(kvp => new GachaBannerClientInfo
            {
                BannerId = kvp.Key,
                PityThreshold = kvp.Value.PityThreshold,
                // 만약 Remote Config에 displayName, cost 등이 있다면 여기서 매핑합니다.
                // DisplayName = kvp.Value.DisplayName, 
                // CostSingle = kvp.Value.CostSingle
            }).ToList();

            return clientBanners;
        }

        // --- 실제 가챠 로직을 수행하는 공통 함수 ---
        private async Task<GachaResult> PerformGachaDraw(IExecutionContext context, IGameApiClient gameApiClient, string bannerId, int drawCount)
        {
            // --- 추가된 부분 ---
            // PlayerId가 없으면 가챠를 진행할 수 없으므로, 명확한 오류를 발생시키고 함수를 중단합니다.
            if (string.IsNullOrEmpty(context.PlayerId))
            {
                _logger.LogError("Player ID is not available in the current context. This function must be called by a player.");
                throw new InvalidOperationException("Player ID is not available in the current context. This function must be called by a player.");
            }
            // 이 검사를 통과하면, 컴파일러는 이 아래부터 context.PlayerId가 절대 null이 아님을 인지합니다.
            // 따라서 더 이상 경고가 발생하지 않습니다.


            // 1. 전체 가챠 설정을 Remote Config에서 불러와 캐싱
            await InitializeGachaConfigAsync(context, gameApiClient);

            // 2. 요청된 bannerId에 해당하는 설정을 가져오기
            if (!s_gachaConfig.TryGetValue(bannerId, out var bannerConfig))
            {
                _logger.LogError($"Invalid bannerId: {bannerId}");
                throw new Exception($"Invalid bannerId: {bannerId}");
            }

            // 3. bannerId를 사용하여 동적인 Cloud Save 키 생성
            string pityKey = PITY_COUNT_KEY_PREFIX + bannerId;
            int currentPityCount = await LoadPityCount(context, gameApiClient, pityKey);

            // 2. 재화 차감 (Economy Virtual Purchase)
            string purchaseId = drawCount == 1 ? GACHA_PURCHASE_ID_SINGLE : GACHA_PURCHASE_ID_TEN;
            var purchaseRequest = new PlayerPurchaseVirtualRequest(purchaseId);
            try
            {
                await gameApiClient.EconomyPurchases.MakeVirtualPurchaseAsync(context, context.AccessToken, context.ProjectId, context.PlayerId, purchaseRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex} => Virtual Purchase failed. Cancel Gacha Logic.");
                throw; 
            }
            
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

                    // 딱히 정해진 확정 지정 유닛이 없다면(GuaranteedItemId = -1), Epic 아무거나 지급
                    if (bannerConfig.GuaranteedItemId == -1)
                    {
                        // 1. 현재 배너의 RarityTable에서 Epic 등급에 대한 정보를 찾습니다.
                        var epicRarityInfo = bannerConfig.RarityTable
                            .FirstOrDefault(r => r.RarityType == Rarity.Epic);

                        // 2. Epic 등급 정보가 없거나, Epic 등급에 속한 유닛 ID가 하나도 없다면 설정 오류이므로 예외를 발생시킵니다.
                        if (epicRarityInfo == null || !epicRarityInfo.IDs.Any())
                        {
                            throw new InvalidOperationException(
                                $"Pity triggered for banner '{bannerId}', but no Epic items are defined in the rarity table. Check Remote Config.");
                        }

                        // 3. Epic 등급의 ID 리스트에서 무작위로 하나의 아이템을 선택합니다.
                        int randomIndex = s_rand.Next(0, epicRarityInfo.IDs.Count);
                        selectedItemId = epicRarityInfo.IDs[randomIndex];
                    }

                    // 천장 아이템의 등급을 찾아야 로직이 올바르게 동작.. 그냥 Epic이라고 딸깍하면 좋은데..
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
            catch (Exception ex)
            {
                _logger.LogError($"{ex} => Invaild currency will be transfered.");
                // 재화 정보 조회를 실패하더라도 가챠의 핵심 결과는 전달되어야 하므로,
                // 에러를 던지지 않고 기본값(0)을 사용하거나 -1 같은 특정 값으로 표기할 수 있습니다.
                finalUserCurrency = -1; // 조회 실패를 의미
            }


            // 5. 최종 천장 카운트를 Cloud Save에 저장   
            try 
            {
                await gameApiClient.CloudSaveData.SetItemAsync(context, context.AccessToken, context.ProjectId, context.PlayerId, new SetItemBody(pityKey, pityCountForLoop));
            } 
            catch (Exception ex)
            {
                _logger.LogError($"{ex} => failed saving pity info.");
                finalUserCurrency = -1;
            }

            // 6. 결과 반환
            return new GachaResult
            {
                ResultUnit = rewardedUnits,
                CurrentPityCount = pityCountForLoop,
                UserCurrency = finalUserCurrency
            };
        }


        //Remote Config에서 확률표를 불러와 캐싱하는 초기화 함수
        private async Task InitializeGachaConfigAsync(IExecutionContext context, IGameApiClient gameApiClient)
        {
            // 이미 테이블이 초기화되었다면 아무것도 하지 않고 즉시 반환 (캐싱)
            if (s_gachaConfig != null && s_gachaConfig.Count > 0)
            {
                return;
            }

            // 동시성 제어: 여러 요청이 동시에 들어와도 단 하나의 요청만 테이블을 초기화하도록 보장
            lock (s_configLock)
            {
                // lock 내부에서 한 번 더 확인 (Double-checked locking)
                if (s_gachaConfig != null && s_gachaConfig.Count > 0)
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
                _logger.LogError($"{ex}, Failed to initialize gacha table from Remote Config.");
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
                _logger.LogWarning("Failed to load data from cloud. Pity count wil be zero");
                return 0; // 데이터가 없는 정상적인 경우
            }
            return 0; // 예외 발생 시 안전하게 0으로 처리
        }

        private Dictionary<string, GachaBannerConfig> ConvertToGachaBannerConfigs(Dictionary<string, object> remoteConfigData)
        {
            var gachaBanners = new Dictionary<string, GachaBannerConfig>();

            // 1. 타입을 JObject로 체크합니다.
            if (remoteConfigData.TryGetValue(GACHA_TABLE_CONFIG_KEY, out var gachaDataObj) && gachaDataObj is JObject gachaDataJObject)
            {
                // 2. JObject에서 "gachaBanners" 키로 값을 가져옵니다. JObject의 값도 JObject일 수 있습니다.
                if (gachaDataJObject.TryGetValue("gachaBanners", out var bannersToken) && bannersToken is JObject bannersJObject)
                {
                    // 3. 최종적으로 얻은 JObject를 Dictionary<string, GachaBannerConfig>로 변환합니다.
                    // 이 한 줄이 bannersJObject를 순회하며 GachaBannerConfig로 변환하는 모든 로직을 대체합니다.
                    gachaBanners = bannersJObject.ToObject<Dictionary<string, GachaBannerConfig>>();

                    if (gachaBanners != null)
                    {
                        // BannerId를 설정해주는 후처리 로직 (필요한 경우)
                        foreach (var entry in gachaBanners)
                        {
                            entry.Value.BannerId = entry.Key;
                        }
                    }

                    else
                    {
                        throw new Exception("[ConvertToGachaBannerConfigs] Cannot Convert remoteConfigData");
                    }
                }
                else
                {
                    _logger.LogError("case 2: \"GACHA_BANNERS\" exists, but no \"gachaBanners\" key or its value is not a JObject");
                }
            }
            else
            {
                _logger.LogError("case 1: no \"GACHA_BANNERS\" key found or its value is not a JObject");
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




