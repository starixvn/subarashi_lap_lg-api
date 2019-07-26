using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fanex.Athena.LGAPI.Core.Model.DTO.RNGCasino
{
    public class GameType
    {
        public int GameTypeId { get; set; }

        public string GameTypeName { get; set; }

        public IList<StakeLevel> StakeLevelList { get; set; }
    }
}
