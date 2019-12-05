using System;
using System.Collections.Generic;
using System.Linq;
using CitiXchangeLibrary;
using Fanex.Athena.LGAPI.Core.Constants;
using Fanex.Athena.LGAPI.Core.Model.DataDomain;
using Fanex.Athena.LGAPI.Core.Model.DTO.Exchange;
using Fanex.Athena.LGAPI.Core.Model.MetalModel.Exchange;
using Fanex.Caching;
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
        private const int ExpirationTimeInMinute = 5;

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
            if (!isValidSiteName || IsUM())
            {
                return new BetHistoryResult
                {
                    DataList = new List<BetHistoryItem>(),
                    ErrCode = isValidSiteName ? CodeForUM : SuccessCode
                };
            }

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

            return _breaker.Execute(
                () => _apiClient.queryBetHistory(metaModel.CustId, startDate, endDate, BetStatusHistory.VOIDED, false, metaModel.PageIndex, env));
        }

        private void CacheApiResult(string cacheKey, BetHistory exchangeResult, ExchangeGettingVoidedMetaModel metaModel)
        {
            var shouldBeCached = exchangeResult.status == CitiStatus.Success &&
                                DateTime.Compare(DateTime.Now.Date, metaModel.SelectedDate.Date) > 0 &&
                                metaModel.CustId == 0;

            if (shouldBeCached)
            {
                var cachePolicy = new CacheItemOptions().SetSlidingExpiration(TimeSpan.FromMinutes(ExpirationTimeInMinute));
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
                var hasRefNo = metaModel.RefNo > 0;
                var siteId = _siteNameMap[metaModel.SiteName];

                return resultList
                    .Where(item => item.extension1 == siteId && (!hasRefNo || item.betId == metaModel.RefNo))
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
    }
}