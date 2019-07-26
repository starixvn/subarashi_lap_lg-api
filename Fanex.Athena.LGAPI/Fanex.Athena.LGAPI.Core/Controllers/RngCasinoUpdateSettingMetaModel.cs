using System.Collections.Generic;
using Fanex.Athena.LGAPI.Core.Model.DTO.RNGCasino;

namespace Fanex.Athena.LGAPI.Core.Controllers
{
    public class RngCasinoUpdateSettingMetaModel
    {
        public IDictionary<string, string> StakeLevelList { get; set; }

        public string RequestIP { get; set; }

        public int CustId { get; set; }

        public int ModifyById { get; set; }

        public string ModifyByUsername { get; set; }
    }
}