using System;
using System.Collections.Generic;
using System.Linq;
using CitiXchangeLibrary;
using Fanex.Athena.LGAPI.Core.Constants;
using Fanex.Athena.LGAPI.Core.Model.DataDomain;
using Fanex.Athena.LGAPI.Core.Model.DTO.Exchange;
using Fanex.Athena.LGAPI.Core.Model.MetalModel.Exchange;
using Polly;

namespace Fanex.Athena.LGAPI.Core.Service.Exchange
{
    public class ExchangeBetHistoryService : IExchangeBetHistoryService
    {
        private readonly ICitiXchangeClient _apiClient;

        private readonly ISyncPolicy<BetHistory> _breaker;

        private const string DateTimeFormat = "yyyy-MM-dd HH:mm";
        private const int CodeForUM = 101;

        private static readonly IDictionary<string, string> _siteNameMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { SiteName.OneTwoBet, ExchangeSiteId.OneTwoBet },
            { SiteName.ALog, ExchangeSiteId.ALog }
        };

        public ExchangeBetHistoryService(ICitiXchangeClient apiClient, ISyncPolicy<BetHistory> breaker)
        {
            _apiClient = apiClient;
            _breaker = breaker;
        }

        public BetHistoryResult GetVoidedData(ExchangeGettingVoidedMetaModel metaModel)
        {
            var isValidSiteName = !_siteNameMap.ContainsKey(metaModel.SiteName);
            if (!isValidSiteName || IsUM())
            {
                return new BetHistoryResult
                {
                    DataList = new List<BetHistoryItem>(),
                    ErrCode = isValidSiteName ? 0 : CodeForUM
                };
            }

            var startDate = metaModel.SelectedDate.Date.ToString(DateTimeFormat);
            var endDate = metaModel.SelectedDate.Date.AddDays(1).AddSeconds(-1).ToString(DateTimeFormat);
            var env = ExchangeEnvironment.EnvironmentMap[metaModel.Environment];

            var exchangeResult = _breaker.Execute(
                () => _apiClient.queryBetHistory(metaModel.CustId, startDate, endDate, BetStatusHistory.VOIDED, false, metaModel.PageIndex, env));

            return BuildResult(exchangeResult, metaModel);
        }

        private static BetHistoryResult BuildResult(BetHistory exchangeResult, ExchangeGettingVoidedMetaModel metaModel)
        {
            var result = new BetHistoryResult();
            var dataList = new List<BetHistoryItem>();
            if (exchangeResult != null && exchangeResult.resultList != null && exchangeResult.resultList.Any())
            {
                var hasUserId = metaModel.CustId > 0;
                var userId = metaModel.CustId.ToString();
                var hasRefNo = metaModel.RefNo > 0;
                var siteId = _siteNameMap[metaModel.SiteName];

                dataList = exchangeResult.resultList
                    .Where(item => item.extension1 == siteId &&
                        (!hasUserId || item.userId == userId) && (!hasRefNo || item.betId == metaModel.RefNo))
                    .Select(item => new BetHistoryItem
                    {
                        RefNo = item.betId,
                        EventType = item.eventTypeName,
                        Event = item.eventName
                    }).ToList();

                result.TotalPage = exchangeResult.totalPage;
            }

            result.DataList = dataList;

            return result;
        }

        private bool IsUM()
        {
            try
            {
                return _apiClient.isUnderMaintainence();
            }
            catch
            {
                return true;
            }
        }
    }
}