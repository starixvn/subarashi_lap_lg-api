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

            var exchangeResult = _breaker.Execute(() => _apiClient.querySettleBetHistory(metaModel.CustId, startDate, endDate, metaModel.PageNumber, env));

            return BuildResult(exchangeResult);
        }

        private static SettleBetHistoryResult BuildResult(SettleBetHistory exchangeResult)
        {
            var result = new SettleBetHistoryResult { TotalPage = (int)exchangeResult.totalPage };
            var dataList = new List<SettleBetHistoryItem>();
            if (exchangeResult != null && exchangeResult.resultList != null && exchangeResult.resultList.Any())
            {
                foreach (var item in exchangeResult.resultList)
                {
                    if (item.betStatus?.ToLowerInvariant() == VoidedStatus)
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
    }
}