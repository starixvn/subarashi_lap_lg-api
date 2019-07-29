using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fanex.Athena.LGAPI.Core.Model.DTO.RNGCasino
{
    [Serializable]
    public class StakeLevel
    {
        public int StakeLevelId { get; set; }

        public string StakeLevelName { get; set; }

        public string StakeLevelNameFormated
        {
            get
            {
                return StakeLevelName ?? string.Empty;
            }
        }

        public bool IsCheck { get; set; }
    }
}
