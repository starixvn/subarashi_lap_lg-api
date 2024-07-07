using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Fanex.Athena.LGAPI.Core.Model.DataDomain;
using Fanex.Athena.LGAPI.Core.Model.DTO.Exchange;
using Fanex.Athena.LGAPI.Core.Model.MetalModel.Exchange;
using Fanex.Caching;
using Fanex.Logging;
using Fanex.ThirdParty.ExchangeProduct.Enums;
using Fanex.ThirdParty.ExchangeProduct.HttpClients;
using Fanex.ThirdParty.ExchangeProduct.Model;
using Fanex.ThirdParty.ExchangeProduct.Params;
using Polly;

namespace Fanex.Athena.LGAPI.Core.Service.Exchange
{
    public class ExchangeBetHistoryService : ExchangeCommonService, IExchangeBetHistoryService
    {
        private readonly IAsyncPolicy<BetHistory> _breaker;
        private readonly IExchangeSetting _setting;

        private const int DefaultTimeZone = 8;
        private const int ExpirationTimeInMinute = 10;
        private const int PageSize = 2000;
        private const int SuccessCode = (int)CitiStatus.Success;
        private const int UMCode = (int)CitiStatus.Under_Maintainence;
        protected const string DateTimeFormat = "yyyy-MM-dd HH:mm";
        private const int DelayTime = 1000;
        private static readonly int[] reportTypes = new[] { ExchangeReportConstants.Exchange, ExchangeReportConstants.Sportsbook, ExchangeReportConstants.Bookmaker, ExchangeReportConstants.FancyBet };

        private IEnumerable<ExchangeLicenseeMapping> SiteNameMapping => _setting.ExchangeLicenseeMappings;

        public ExchangeBetHistoryService(IExchangeHttpClient apiClient, IAsyncPolicy<BetHistory> breaker, ICacheService cacheService, IExchangeSetting setting, ILogger logger)
            : base(apiClient, logger, cacheService)
        {
            _breaker = breaker;
            _setting = setting;
        }

        public async Task<BetHistoryResult> GetVoidedData(ExchangeGettingVoidedMetaModel metaModel)
        {
            var isUM = await IsUnderMaintenance();

            if (isUM)
            {
                return new BetHistoryResult(UMCode);
            }

            if (!IsValidParam(metaModel))
            {
                return new BetHistoryResult(SuccessCode);
            }

            var totalItemsByType = (await Task.WhenAll(reportTypes.Select(async r => await GetTotalItems(metaModel, r)))).ToList();
            var failed = totalItemsByType.FirstOrDefault(t => !IsSuccess(t.Status));
            if (failed != null)
            {
                return new BetHistoryResult(failed.Status);
            }

            await GetAllDataByCustId(metaModel, totalItemsByType);

            var apiData = GetCustDataBySite(metaModel, totalItemsByType);

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

        private bool IsValidParam(ExchangeGettingVoidedMetaModel metaModel)
        {
            var isValidSiteName = SiteNameMapping.Any(_ => _.Name.Equals(metaModel.SiteName, StringComparison.InvariantCultureIgnoreCase));
            var isValidRefno = string.IsNullOrEmpty(metaModel.RefNo) || long.TryParse(metaModel.RefNo, out _);
            return isValidSiteName && isValidRefno;
        }

        private async Task<ReportTypeItemsDTO> GetTotalItems(ExchangeGettingVoidedMetaModel metaModel, int reportType)
        {
            var param = metaModel.Clone();
            param.ReportType = reportType;
            param.PageIndex = 1;

            var result = new ReportTypeItemsDTO { ReportType = reportType };
            var apiData = await _breaker.ExecuteAsync(async () => await QueryBetHistory(param));
            if (apiData != null)
            {
                result.Status = (int)apiData.status;
                result.TotalPage = apiData.totalPage;
            }

            return result;
        }

        private async Task<BetHistory> QueryBetHistory(ExchangeGettingVoidedMetaModel param)
        {
            FormatAPIParam(param);
            var queryParam = new QueryBetHistoryForAllStatusParam
            {
                CustId = param.CustId,
                BetStatus = BetStatusHistory.VOIDED,
                StartDate = DateTime.ParseExact(param.StartTime, DateTimeFormat, CultureInfo.InvariantCulture),
                EndDate = DateTime.ParseExact(param.EndTime, DateTimeFormat, CultureInfo.InvariantCulture),
                GroupMode = param.GroupMode,
                IsTxnDetail = true,
                PageNumber = param.PageIndex,
                ReportType = param.ReportType,
            };

            var apiResult = await _apiClient.QueryBetHistoryForAllStatusAsync(queryParam);
            await Task.Delay(DelayTime);
            return apiResult.Result.voided;
        }

        private static void FormatAPIParam(ExchangeGettingVoidedMetaModel param)
        {
            var diffHours = DefaultTimeZone - param.TimeZone;
            var selectedDate = param.SelectedDate.Date.AddHours(diffHours);

            param.StartTime = selectedDate.ToString(DateTimeFormat);
            param.EndTime = selectedDate.AddDays(1).ToString(DateTimeFormat);
            param.GroupMode = string.Equals(SiteName.OneTwoBet, param.SiteName, StringComparison.OrdinalIgnoreCase) ? (int)GroupMode.OneTwoBet : (int)GroupMode.Other;
        }

        private static bool IsSuccess(int status) => status == SuccessCode || status == 0;

        private async Task GetAllDataByCustId(ExchangeGettingVoidedMetaModel metaModel, List<ReportTypeItemsDTO> totalItemsByType)
        {
            var cacheKey = $"ExchangeVoided_{metaModel.SelectedDateStr}_{metaModel.CustId}_0_1";
            if (_cacheService.Get<BetHistory>(cacheKey) != null)
            {
                return;
            }

            var param = metaModel.Clone();

            for (var i = 0; i < reportTypes.Length; i++)
            {
                var totalPage = totalItemsByType[i].TotalPage;
                var t = reportTypes[i];
                param.ReportType = t;
                param.PageIndex = 1;

                while (param.PageIndex <= totalPage)
                {
                    var apiData = await _breaker.ExecuteAsync(async () => await QueryBetHistory(param));
                    var shouldBeCached = IsSuccess((int)apiData.status) && apiData.resultList.Any();
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
            var siteId = SiteNameMapping.FirstOrDefault(_ => _.Name.Equals(param.SiteName, StringComparison.InvariantCultureIgnoreCase))?.ExchangeId;
            if (siteId == null)
            {
                return Enumerable.Empty<BetHistoryResultList>();
            }
            var key = $"ExchangeVoided_{param.SelectedDateStr}_{param.CustId}_{siteId}";
            var dataList = _cacheService.Get<IEnumerable<BetHistoryResultList>>(key);
            if (dataList != null && dataList.Any())
            {
                return dataList;
            }

            dataList = new List<BetHistoryResultList>();
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

                    dataList = dataList.Union(apiData.resultList.Where(r => r.extension1 == siteId.ToString()));
                }
            }

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