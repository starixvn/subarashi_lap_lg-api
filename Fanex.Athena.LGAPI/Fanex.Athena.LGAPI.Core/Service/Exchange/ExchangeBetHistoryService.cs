using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CitiXchangeLibrary;
using Fanex.Athena.LGAPI.Core.Constants;
using Fanex.Athena.LGAPI.Core.Model.DataDomain;
using Fanex.Athena.LGAPI.Core.Model.DTO.Exchange;
using Fanex.Athena.LGAPI.Core.Model.MetalModel.Exchange;
using Fanex.Caching;
using Fanex.Logging;
using Polly;

namespace Fanex.Athena.LGAPI.Core.Service.Exchange
{
    public class ExchangeBetHistoryService : IExchangeBetHistoryService
    {
        private readonly ICitiXchangeClient _apiClient;

        private readonly ISyncPolicy<BetHistory> _breaker;

        private readonly ICacheService _cacheService;

        private const string DateTimeFormat = "yyyy-MM-dd HH:mm";
        private const int CodeForUM = 101;
        private const int SuccessCode = (int)CitiStatus.Success;
        private const int ExpirationTimeInMinute = 2;

        private static readonly IDictionary<string, string> _siteNameMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { SiteName.OneTwoBet, ExchangeSiteId.OneTwoBet },
            { SiteName.ALog, ExchangeSiteId.ALog }
        };

        public ExchangeBetHistoryService(ICitiXchangeClient apiClient, ISyncPolicy<BetHistory> breaker, ICacheService cacheService)
        {
            _apiClient = apiClient;
            _breaker = breaker;
            _cacheService = cacheService;
        }

        public BetHistoryResult GetVoidedData(ExchangeGettingVoidedMetaModel metaModel)
        {
            var isValidSiteName = _siteNameMap.ContainsKey(metaModel.SiteName);
            var isValidRefno = IsValidRefNo(metaModel.RefNo, out int refNo);
            if (!isValidSiteName || !isValidRefno || IsUM())
            {
                var errCode = (isValidSiteName && isValidRefno) ? CodeForUM : SuccessCode;
                return CreateEmptyResult(errCode);
            }

            if (metaModel.CustId == 0)
            {
                var dataByDay = GetAllByDay(metaModel);
                var shouldSearchMore = refNo > 0 && dataByDay.TotalPage > metaModel.PageIndex && !dataByDay.DataList.Any();

                return shouldSearchMore ? QueryByRefNo(metaModel, dataByDay.TotalPage) : dataByDay;
            }
            else
            {
                var apiResult = GetApiData(metaModel);

                return BuildResult(apiResult, metaModel);
            }
        }

        private BetHistoryResult QueryByRefNo(ExchangeGettingVoidedMetaModel metaModel, int totalPage)
        {
            var pageIndex = metaModel.PageIndex + 1;
            for (var i = pageIndex; i <= totalPage; i++)
            {
                var metaModelByPage = BuildMetaModelByPage(metaModel, i);
                var searchedResult = GetAllByDay(metaModelByPage);

                if (searchedResult.DataList.Any())
                {
                    return searchedResult;
                }
            }

            return CreateEmptyResult();
        }

        private static BetHistoryResult CreateEmptyResult(int errCode = SuccessCode)
        {
            return new BetHistoryResult
            {
                DataList = new List<BetHistoryItem>(),
                ErrCode = errCode
            };
        }

        private static ExchangeGettingVoidedMetaModel BuildMetaModelByPage(ExchangeGettingVoidedMetaModel metaModel, int pageIndex)
        {
            return new ExchangeGettingVoidedMetaModel
            {
                CustId = metaModel.CustId,
                Environment = metaModel.Environment,
                RefNo = metaModel.RefNo,
                SelectedDate = metaModel.SelectedDate,
                SiteName = metaModel.SiteName,
                PageIndex = pageIndex
            };
        }

        private BetHistoryResult GetAllByDay(ExchangeGettingVoidedMetaModel metaModel)
        {
            var cacheKey = $"ExchangeVoided_{metaModel.SelectedDate.ToString("yyyyMMdd")}_{metaModel.PageIndex}";
            var exchangeResult = _cacheService.Get<BetHistory>(cacheKey);
            if (exchangeResult == null)
            {
                exchangeResult = GetApiData(metaModel);
                CacheApiResult(cacheKey, exchangeResult, metaModel);
            }

            return BuildResult(exchangeResult, metaModel);
        }

        private BetHistory GetApiData(ExchangeGettingVoidedMetaModel metaModel)
        {
            var selectedDate = metaModel.SelectedDate.Date;
            var startDate = selectedDate.ToString(DateTimeFormat);
            var endDate = selectedDate.AddDays(1).AddSeconds(-1).ToString(DateTimeFormat);
            var env = ExchangeEnvironment.EnvironmentMap[metaModel.Environment];

            var apiResult = _breaker.Execute(
                () => _apiClient.queryBetHistory(metaModel.CustId, startDate, endDate, BetStatusHistory.VOIDED, false, metaModel.PageIndex, env));

            WriteLogForResult(metaModel, apiResult);

            return apiResult;
        }

        private void CacheApiResult(string cacheKey, BetHistory exchangeResult, ExchangeGettingVoidedMetaModel metaModel)
        {
            var shouldBeCached = exchangeResult.status == CitiStatus.Success &&
                                DateTime.Compare(DateTime.Now.Date, metaModel.SelectedDate.Date) > 0;

            if (shouldBeCached)
            {
                var cachePolicy = new CacheItemOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(ExpirationTimeInMinute));
                _cacheService.Set(cacheKey, exchangeResult, cachePolicy);
            }
        }

        private static BetHistoryResult BuildResult(BetHistory exchangeResult, ExchangeGettingVoidedMetaModel metaModel)
        {
            var result = new BetHistoryResult { DataList = new List<BetHistoryItem>() };

            if (exchangeResult != null)
            {
                result.ErrCode = (int)exchangeResult.status;
                result.TotalPage = exchangeResult.totalPage;
                result.DataList = GetDataList(exchangeResult.resultList, metaModel);
            }

            return result;
        }

        private static IEnumerable<BetHistoryItem> GetDataList(List<BetHistoryResultList> resultList, ExchangeGettingVoidedMetaModel metaModel)
        {
            if (resultList != null && resultList.Any())
            {
                var hasRefNo = IsValidRefNo(metaModel.RefNo, out int refNo) && refNo > 0;
                var siteId = _siteNameMap[metaModel.SiteName];

                return resultList
                    .Where(item => item.extension1 == siteId && (!hasRefNo || item.betId == refNo))
                    .Select(item => new BetHistoryItem
                    {
                        RefNo = item.betId,
                        EventType = item.eventTypeName,
                        Event = item.eventName,
                        SiteId = item.extension1,
                        UserId = item.userId
                    });
            }

            return new List<BetHistoryItem>();
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

        public static bool IsValidRefNo(string refNo, out int result)
        {
            result = 0;

            return string.IsNullOrEmpty(refNo) || int.TryParse(refNo, out result);
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