using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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

        private const int CodeForUM = 101;
        private const int ExpirationTimeInMinute = 60;
        private const int PageSize = 2000;
        private const int SuccessCode = (int)CitiStatus.Success;
        protected const string DateTimeFormat = "yyyy-MM-dd HH:mm";
        private const int DelayTime = 1000;
        private static readonly int[] reportTypes = new[] { ExchangeReportConstants.FancyBet, ExchangeReportConstants.Bookmaker };
        private static readonly IDictionary<string, string> _siteNameMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            [SiteName.OneTwoBet] = ExchangeSiteId.OneTwoBet,
            [SiteName.ALog] = ExchangeSiteId.ALog
        };

        public ExchangeBetHistoryService(ICitiXchangeClient apiClient, ISyncPolicy<BetHistory> breaker, ICacheService cacheService)
        {
            _apiClient = apiClient;
            _breaker = breaker;
            _cacheService = cacheService;
        }

        public BetHistoryResult GetVoidedData(ExchangeGettingVoidedMetaModel metaModel)
        {
            if (IsUM())
            {
                return new BetHistoryResult(CodeForUM);
            }

            if (!IsValidParam(metaModel))
            {
                return new BetHistoryResult(SuccessCode);
            }

            var totalItemsByType = reportTypes.Select(r => GetTotalItems(metaModel, r)).ToList();
            var failed = totalItemsByType.FirstOrDefault(t => !IsSuccess(t.Status));
            if (failed != null)
            {
                return new BetHistoryResult(failed.Status);
            }

            GetAllDataByCustId(metaModel, totalItemsByType);

            var siteId = _siteNameMap[metaModel.SiteName];
            var key = $"ExchangeVoided_{metaModel.SelectedDateStr}_{metaModel.CustId}_{siteId}";
            var apiData = _cacheService.Get<IEnumerable<BetHistoryResultList>>(key) ?? GetCustDataBySite(metaModel, totalItemsByType);

            if (apiData == null)
            {
                return new BetHistoryResult(SuccessCode);
            }

            var refNo = string.IsNullOrEmpty(metaModel.RefNo) ? 0 : long.Parse(metaModel.RefNo);
            var dataList = refNo > 0 ? apiData.Where(d => d.betId == refNo) : apiData;

            return new BetHistoryResult
            {
                ErrCode = SuccessCode,
                DataList = dataList.Skip((metaModel.PageIndex - 1) * PageSize).Take(PageSize).Select(item => Create(item)),
                TotalPage = Math.Ceiling((decimal)dataList.Count() / PageSize)
            };
        }

        private static bool IsValidParam(ExchangeGettingVoidedMetaModel metaModel)
        {
            var isValidSiteName = _siteNameMap.ContainsKey(metaModel.SiteName);
            var isValidRefno = string.IsNullOrEmpty(metaModel.RefNo) || int.TryParse(metaModel.RefNo, out _);
            return isValidSiteName && isValidRefno;
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

        private ReportTypeItemsDTO GetTotalItems(ExchangeGettingVoidedMetaModel metaModel, int reportType)
        {
            var param = metaModel.Clone();
            param.ReportType = reportType;
            param.PageIndex = 1;

            var result = new ReportTypeItemsDTO { ReportType = reportType };
            var env = ExchangeEnvironment.EnvironmentMap[param.Environment];
            var apiData = _breaker.Execute(() => QueryBetHistory(param, env));
            if (apiData != null)
            {
                result.Status = (int)apiData.status;
                result.TotalPage = apiData.totalPage;
            }

            return result;
        }

        private BetHistory QueryBetHistory(ExchangeGettingVoidedMetaModel param, Enviro env)
        {
            FormatAPIParam(param);
            var apiResult = _apiClient.queryBetHistory(param.CustId, param.StartTime, param.EndTime, BetStatusHistory.VOIDED, true, param.PageIndex, param.ReportType, env);
            Thread.Sleep(DelayTime);
            return apiResult;
        }

        private static void FormatAPIParam(ExchangeGettingVoidedMetaModel param)
        {
            var selectedDate = param.SelectedDate.Date;
            param.StartTime = selectedDate.ToString(DateTimeFormat);
            param.EndTime = selectedDate.AddDays(1).AddSeconds(-1).ToString(DateTimeFormat);
        }

        private static bool IsSuccess(int status) => status == SuccessCode || status == 0;

        private void GetAllDataByCustId(ExchangeGettingVoidedMetaModel metaModel, List<ReportTypeItemsDTO> totalItemsByType)
        {
            var cacheKey = $"ExchangeVoided_{metaModel.SelectedDateStr}_{metaModel.CustId}_0_1";
            if (_cacheService.Get<BetHistory>(cacheKey) != null)
            {
                return;
            }

            var param = metaModel.Clone();
            var env = ExchangeEnvironment.EnvironmentMap[param.Environment];

            for (var i = 0; i < reportTypes.Length; i++)
            {
                var totalPage = totalItemsByType[i].TotalPage;
                var t = reportTypes[i];
                param.ReportType = t;
                param.PageIndex = 1;

                while (param.PageIndex <= totalPage)
                {
                    var apiData = _breaker.Execute(() => QueryBetHistory(param, env));
                    var shouldBeCached = IsSuccess((int)apiData.status) && DateTime.Compare(DateTime.Now.Date, param.SelectedDate.Date) > 0;
                    cacheKey = $"ExchangeVoided_{param.SelectedDateStr}_{param.CustId}_{param.ReportType}_{param.PageIndex}";

                    if (shouldBeCached)
                    {
                        var cachePolicy = new CacheItemOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(ExpirationTimeInMinute));
                        _cacheService.Set(cacheKey, apiData, cachePolicy);
                    }

                    param.PageIndex++;
                }
            }
        }

        private IEnumerable<BetHistoryResultList> GetCustDataBySite(ExchangeGettingVoidedMetaModel param, List<ReportTypeItemsDTO> totalItemsByType)
        {
            var siteId = _siteNameMap[param.SiteName];
            IEnumerable<BetHistoryResultList> dataList = new List<BetHistoryResultList>();

            for (var i = 0; i < reportTypes.Length; i++)
            {
                var t = reportTypes[i];
                var totalPage = totalItemsByType[i].TotalPage;
                var pageIndex = 1;
                while (pageIndex <= totalPage)
                {
                    var cacheKey = $"ExchangeVoided_{param.SelectedDateStr}_{param.CustId}_{t}_{pageIndex}";
                    var apiData = _cacheService.Get<BetHistory>(cacheKey);
                    pageIndex++;

                    if (apiData == null)
                    {
                        continue;
                    }

                    dataList = dataList.Union(apiData.resultList.Where(r => r.extension1 == siteId));
                }
            }

            var key = $"ExchangeVoided_{param.SelectedDateStr}_{param.CustId}_{siteId}";
            var cachePolicy = new CacheItemOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(ExpirationTimeInMinute));
            _cacheService.Set<IEnumerable<BetHistoryResultList>>(key, dataList.ToList(), cachePolicy);
            return dataList;
        }

        private static BetHistoryItem Create(BetHistoryResultList item)
            => new BetHistoryItem
            {
                RefNo = item.betId,
                EventType = item.eventTypeName,
                Event = item.eventName
            };
    }
}
