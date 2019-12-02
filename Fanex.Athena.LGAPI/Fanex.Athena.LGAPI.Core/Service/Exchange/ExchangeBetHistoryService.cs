using System;
using System.Collections.Generic;
using System.Linq;
using CitiXchangeLibrary;
using Fanex.Athena.LGAPI.Core.Constants;
using Fanex.Athena.LGAPI.Core.Model.DTO.Exchange;
using Fanex.Athena.LGAPI.Core.Model.MetalModel.Exchange;
using Polly.CircuitBreaker;

namespace Fanex.Athena.LGAPI.Core.Service.Exchange
{
    public class ExchangeBetHistoryService : IExchangeBetHistoryService
    {
        private readonly ICitiXchangeClient _apiClient;

        private readonly CircuitBreakerPolicy<BetHistory> _breaker;

        private const string DateTimeFormat = "yyyy-MM-dd HH:mm";
        private const string Gmt8ZoneId = "Taipei Standard Time";
        private const int CodeForUM = 101;

        public ExchangeBetHistoryService(ICitiXchangeClient apiClient, CircuitBreakerPolicy<BetHistory> breaker)
        {
            _apiClient = apiClient;
            _breaker = breaker;
        }

        public BetHistoryResult GetVoidedData(ExchangeGettingVoidedMetaModel metaModel)
        {
            if (IsUM())
            {
                return new BetHistoryResult { DataList = new List<BetHistoryItem>(), ErrCode = CodeForUM };
            }

            var dateRange = ConvertToGMT8(metaModel.SelectedDate);
            var startDate = dateRange.Item1.ToString(DateTimeFormat);
            var endDate = dateRange.Item2.ToString(DateTimeFormat);
            var env = ExchangeEnvironment.EnvironmentMap[metaModel.Environment];

            var exchangeResult = _breaker.Execute(
                () => _apiClient.queryBetHistory(metaModel.CustId, startDate, endDate, BetStatusHistory.VOIDED, false, metaModel.PageIndex, env));

            return BuildResult(exchangeResult, metaModel);
        }

        private static BetHistoryResult BuildResult(BetHistory exchangeResult, ExchangeGettingVoidedMetaModel metaModel)
        {
            var result = new BetHistoryResult { TotalPage = exchangeResult.totalPage };
            var dataList = new List<BetHistoryItem>();
            if (exchangeResult != null && exchangeResult.resultList != null && exchangeResult.resultList.Any())
            {
                var hasUserId = metaModel.CustId > 0;
                var userId = metaModel.CustId.ToString();
                var hasRefNo = metaModel.RefNo > 0;

                dataList = exchangeResult.resultList
                    .Where(item => (!hasUserId || item.userId == userId) && (!hasRefNo || item.betId == metaModel.RefNo))
                    .Select(item => new BetHistoryItem
                    {
                        RefNo = item.betId,
                        EventType = item.eventTypeName,
                        Event = item.eventName
                    }).ToList();
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

        private bool IsUM()
        {
            var result = true;
            try
            {
                result = _apiClient.isUnderMaintainence();
            }
            catch { }

            return result;
        }
    }
}