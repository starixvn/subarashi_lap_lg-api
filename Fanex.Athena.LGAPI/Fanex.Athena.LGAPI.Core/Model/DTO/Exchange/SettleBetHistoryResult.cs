using System.Collections.Generic;

namespace Fanex.Athena.LGAPI.Core.Model.DTO.Exchange
{
    public class SettleBetHistoryResult
    {
        public IList<SettleBetHistoryItem> DataList { get; set; }

        public int TotalPage { get; set; }
    }
}