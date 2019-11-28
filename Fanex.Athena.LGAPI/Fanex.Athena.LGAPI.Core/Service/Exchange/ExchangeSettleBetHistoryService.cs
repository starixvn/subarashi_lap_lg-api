using System;
using System.Collections.Generic;
using System.Linq;
using CitiXchangeLibrary;
using Fanex.Athena.LGAPI.Core.Constants;
using Fanex.Athena.LGAPI.Core.Model.DTO.Exchange;
using Fanex.Athena.LGAPI.Core.Model.MetalModel.Exchange;
using Fanex.Logging;
using Polly.CircuitBreaker;

namespace Fanex.Athena.LGAPI.Core.Service.Exchange
{
    public class ExchangeSettleBetHistoryService : IExchangeSettleBetHistoryService
    {
        private readonly ICitiXchangeClient _apiClient;

        private readonly ILogger _logger;

        private readonly CircuitBreakerPolicy<SettleBetHistory> _breaker;

        private const string DateTimeFormat = "yyyy-MM-dd HH:mm";
        private const string Gmt8ZoneId = "Taipei Standard Time";
        private const string VoidedStatus = "voided";

        public ExchangeSettleBetHistoryService(ICitiXchangeClient apiClient, ILogger logger, CircuitBreakerPolicy<SettleBetHistory> breaker)
        {
            _apiClient = apiClient;
            _logger = logger;
            _breaker = breaker;
        }

        public SettleBetHistoryResult GetVoidedData(ExchangeGettingVoidedMetaModel metaModel)
        {
            var dateRange = ConvertToGMT8(metaModel.SelectedDate);
            var startDate = dateRange.Item1.ToString(DateTimeFormat);
            var endDate = dateRange.Item2.ToString(DateTimeFormat);
            var env = ExchangeEnvironment.EnvironmentMap[metaModel.Environment];

            var exchangeResult = _breaker.Execute(() => env == Enviro.POC ?
                CreateFakeData() :
                _apiClient.querySettleBetHistory(metaModel.CustId, startDate, endDate, metaModel.PageIndex, env));

            return BuildResult(exchangeResult, metaModel);
        }

        private static SettleBetHistoryResult BuildResult(SettleBetHistory exchangeResult, ExchangeGettingVoidedMetaModel metaModel)
        {
            var result = new SettleBetHistoryResult { TotalPage = (int)exchangeResult.totalPage };
            var dataList = new List<SettleBetHistoryItem>();
            if (exchangeResult != null && exchangeResult.resultList != null && exchangeResult.resultList.Any())
            {
                var hasUserId = metaModel.CustId > 0;
                var userId = metaModel.CustId.ToString();
                var hasRefNo = metaModel.RefNo > 0;

                foreach (var item in exchangeResult.resultList)
                {
                    if (item.betStatus?.ToLowerInvariant() == VoidedStatus &&
                        (!hasUserId || item.userId == userId) && (!hasRefNo || item.betId == metaModel.RefNo))
                    {
                        dataList.Add(new SettleBetHistoryItem
                        {
                            RefNo = item.betId,
                            EventType = item.eventTypeName,
                            Event = item.eventName
                        });
                    }
                }
            }

            result.DataList = dataList;

            return result;
        }

        private static Tuple<DateTime, DateTime> ConvertToGMT8(DateTime selectedDate)
        {
            var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(Gmt8ZoneId);
            var localTimeInUTC = selectedDate.Date.ToUniversalTime();

            var startDate = TimeZoneInfo.ConvertTimeFromUtc(localTimeInUTC, timeZoneInfo);
            var endDate = startDate.AddDays(1).AddSeconds(-1);

            return Tuple.Create(startDate, endDate);
        }

        private static SettleBetHistory CreateFakeData()
        {
            return new SettleBetHistory
            {
                currentPage = 1,
                totalPage = 1,
                status = CitiStatus.Success,
                resultList = new List<BetHistoryResultList>
                {
                    new BetHistoryResultList{ betId = 1, eventTypeName = "Event Type 1", eventName = "Liverppol vs ManCity", betStatus = "voided", userId = "1063580" },
                    new BetHistoryResultList{ betId = 2, eventTypeName = "Event Type 2", eventName = "Liverppol vs Tot", betStatus = "voided", userId = "2533651" },
                    new BetHistoryResultList{ betId = 3, eventTypeName = "Event Type 1", eventName = "Liverppol vs Barca", betStatus = "voided", userId = "3227401" }
                }
            };
        }
    }
}