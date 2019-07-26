using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fanex.Athena.LGAPI.Core.Model.MetalModel.RNGCasino;

namespace Fanex.Athena.LGAPI.Core.Service.RNGCasino
{
    public interface IBasicInfoApiWrapper
    {
        DataTable GetGameKind(string casinoNo);

        DataTable GetGameType(GetGameTypeMetalModel input);
    }
}
