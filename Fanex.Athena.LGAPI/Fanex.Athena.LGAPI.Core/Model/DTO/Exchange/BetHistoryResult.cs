using System.Collections.Generic;

namespace Fanex.Athena.LGAPI.Core.Model.DTO.Exchange
{
    public class BetHistoryResult
    {
        public IList<BetHistoryItem> DataList { get; set; }

        public int TotalPage { get; set; }
    }
}