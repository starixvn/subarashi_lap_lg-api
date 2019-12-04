using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CitiXchangeLibrary;
using Fanex.Athena.LGAPI.Core.Constants;
using Fanex.Athena.LGAPI.Core.Model.DataDomain;
using Fanex.Athena.LGAPI.Core.Model.DTO.Exchange;
using Fanex.Athena.LGAPI.Core.Model.MetalModel.Exchange;
using Fanex.Logging;
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
            var isValidSiteName = _siteNameMap.ContainsKey(metaModel.SiteName);
            if (!isValidSiteName || IsUM())
            {
                return new BetHistoryResult
                {
                    DataList = new List<BetHistoryItem>(),
                    ErrCode = !isValidSiteName ? 0 : CodeForUM
                };
            }

            var startDate = metaModel.SelectedDate.Date.ToString(DateTimeFormat);
            var endDate = metaModel.SelectedDate.Date.AddDays(1).AddSeconds(-1).ToString(DateTimeFormat);
            var env = ExchangeEnvironment.EnvironmentMap[metaModel.Environment];

            var exchangeResult = _breaker.Execute(
                () => _apiClient.queryBetHistory(metaModel.CustId, startDate, endDate, BetStatusHistory.VOIDED, false, metaModel.PageIndex, env));

            WriteLogForResult(metaModel, exchangeResult);

            return BuildResult(exchangeResult, metaModel);
        }

        private static BetHistoryResult BuildResult(BetHistory exchangeResult, ExchangeGettingVoidedMetaModel metaModel)
        {
            var result = new BetHistoryResult();
            var dataList = new List<BetHistoryItem>();
            if (exchangeResult != null && exchangeResult.resultList != null && exchangeResult.resultList.Any())
            {
                var hasRefNo = metaModel.RefNo > 0;
                var siteId = _siteNameMap[metaModel.SiteName];

                dataList = exchangeResult.resultList
                    .Where(item => item.extension1 == siteId && (!hasRefNo || item.betId == metaModel.RefNo))
                    .Select(item => new BetHistoryItem
                    {
                        RefNo = item.betId,
                        EventType = item.eventTypeName,
                        Event = item.eventName,
                        SiteId = item.extension1,
                        UserId = item.userId
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

        private static void WriteLogForResult(ExchangeGettingVoidedMetaModel metaModel, BetHistory exchangeResult)
        {
            var logBuilder = new StringBuilder();
            logBuilder.AppendLine("PARAM INFO:");
            logBuilder.AppendLine($"CustId={metaModel.CustId}&SelectedDate={metaModel.SelectedDate}&PageIndex={metaModel.PageIndex}&Environment={metaModel.Environment}");

            logBuilder.AppendLine("RESULT INFO:");
            if (exchangeResult != null)
            {
                logBuilder.AppendLine($"Status={exchangeResult.status}&ItemCount={exchangeResult.resultList?.Count}&TotalPage={exchangeResult.totalPage}");

                if (exchangeResult.resultList != null && exchangeResult.resultList.Any())
                {
                    var itemCount = Math.Min(exchangeResult.resultList.Count, 10);

                    for (var i = 0; i < itemCount; i++)
                    {
                        var item = exchangeResult.resultList[i];
                        logBuilder.AppendLine($"UserId={item.userId}&RefNo={item.betId}");
                    }
                }
            }

            LogService.Info(logBuilder.ToString());
        }
    }
}