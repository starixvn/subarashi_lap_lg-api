using System;

namespace Fanex.Athena.LGAPI.Core.Model.MetalModel.Exchange
{
    public class ExchangeGettingVoidedMetaModel : ExchangeBaseMetaModel
    {
        public DateTime SelectedDate { get; set; }

        public int PageIndex { get; set; }

        public string RefNo { get; set; }

        public string SiteName { get; set; }

        public int ReportType { get; set; }

        public string SelectedDateStr => SelectedDate.ToString("yyyyMMdd");
        public string StartTime { get; set; }
        public string EndTime { get; set; }

        public ExchangeGettingVoidedMetaModel Clone()
            => new ExchangeGettingVoidedMetaModel
            {
                SelectedDate = SelectedDate,
                PageIndex = PageIndex,
                RefNo = RefNo,
                SiteName = SiteName,
                ReportType = ReportType,
                CustId = CustId,
                Environment = Environment
            };
    }
}
