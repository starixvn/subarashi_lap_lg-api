using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fanex.Athena.LGAPI.Core.Model.MetalModel.RNGCasino
{
    public class RngCasinoSettingMetaModel
    {
        public int CustId { get; set; }
        public int CurrencyID { get; set; }
        public string Language { get; set; }
        public int UserId { get; set; }

        public int Status { get; set; }

        public string Ip { get; set; }
    }
}
