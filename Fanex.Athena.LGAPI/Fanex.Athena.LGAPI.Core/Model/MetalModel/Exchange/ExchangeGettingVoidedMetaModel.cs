using System;

namespace Fanex.Athena.LGAPI.Core.Model.MetalModel.Exchange
{
    public class ExchangeGettingVoidedMetaModel : ExchangeBaseMetaModel
    {
        public DateTime SelectedDate { get; set; }

        public int PageIndex { get; set; }

        public int RefNo { get; set; }

        public string SiteName { get; set; }
    }
}