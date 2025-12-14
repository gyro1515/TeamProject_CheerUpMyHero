using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.CloudCode.Apis;
using Unity.Services.CloudCode.Core;
using Unity.Services.CloudCode.Shared;
using Unity.Services.Economy.Model;

namespace CheerUpMyHero.CloudCode
{
    public class EconomyModule
    {
        private readonly ILogger<EconomyModule> _logger;

        public EconomyModule(ILogger<EconomyModule> logger)
        {
            _logger = logger;
        }

        [CloudCodeFunction("ChangeEconomyResource")]
        public async Task<int> ChangeEconomyResource(IExecutionContext context, IGameApiClient gameApiClient, string currencyId, int amount)
        {
            try
            {
                if (string.IsNullOrEmpty(context.PlayerId))
                {
                    throw new InvalidOperationException("Player ID is not available.");
                }

                if (amount == 0)
                {
                    // 0이면 변경 없이 종료. (필요 시 현재 잔액 조회 로직 추가 가능)
                    return 0;
                }

                CurrencyBalanceResponse newBalanceData;

                var balanceRequest = new CurrencyModifyBalanceRequest(amount: Math.Abs(amount));

                if (amount > 0)
                {
                    var result = await gameApiClient.EconomyCurrencies.IncrementPlayerCurrencyBalanceAsync(
                        context,
                        context.AccessToken,
                        context.ProjectId,
                        context.PlayerId,
                        currencyId,
                        balanceRequest
                    );
                    newBalanceData = result.Data;
                }
                else
                {
                    var result = await gameApiClient.EconomyCurrencies.DecrementPlayerCurrencyBalanceAsync(
                        context,
                        context.AccessToken,
                        context.ProjectId,
                        context.PlayerId,
                        currencyId,
                        balanceRequest
                    );
                    newBalanceData = result.Data;
                }

                // [수정 포인트 4] 반환값 처리
                // 서버 모델의 Balance는 long 타입일 수 있으므로 int 캐스팅 주의
                return (int)newBalanceData.Balance;
            }
            catch (ApiException ex)
            {
                _logger.LogError($"Economy operation failed: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected error: {ex.Message}");
                throw;
            }
        }
    }

    public class EconomyModuleConfig : ICloudCodeSetup
    {
        public void Setup(ICloudCodeConfig config)
        {
            config.Dependencies.AddSingleton(GameApiClient.Create());
        }
    }
}
