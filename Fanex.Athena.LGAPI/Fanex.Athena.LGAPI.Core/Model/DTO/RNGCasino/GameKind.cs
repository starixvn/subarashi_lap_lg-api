using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fanex.Athena.LGAPI.Core.Model.DTO.RNGCasino
{
    [Serializable]
    public class GameKind
    {
        public int GameKindId { get; set; }

        public string GameKindName { get; set; }

        public IList<GameType> GameTypeList { get; set; }
    }
}
