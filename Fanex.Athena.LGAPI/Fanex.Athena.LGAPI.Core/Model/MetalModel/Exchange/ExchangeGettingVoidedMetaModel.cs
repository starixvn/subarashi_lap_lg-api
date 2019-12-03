using System;

namespace Fanex.Athena.LGAPI.Core.Model.MetalModel.Exchange
{
    public class ExchangeGettingVoidedMetaModel
    {
        public int CustId { get; set; }

        public DateTime SelectedDate { get; set; }

        public int PageIndex { get; set; }

        public string Environment { get; set; }

        public int RefNo { get; set; }

        public string SiteId { get; set; }
    }
}