namespace Fanex.Athena.LGAPI.Core.Model.DTO.Exchange
{
    using System.Collections.Generic;

    public class BetHistoryResult
    {
        public IEnumerable<BetHistoryItem> DataList { get; set; }

        public decimal TotalPage { get; set; }

        public int ErrCode { get; set; }

        public BetHistoryResult()
        {
        }

        public BetHistoryResult(int errCode)
        {
            DataList = new List<BetHistoryItem>();
            ErrCode = errCode;
        }

        public BetHistoryResult(BetHistoryResult source, IEnumerable<BetHistoryItem> dataList)
        {
            TotalPage = source.TotalPage;
            ErrCode = source.ErrCode;
            DataList = dataList;
        }
    }
}