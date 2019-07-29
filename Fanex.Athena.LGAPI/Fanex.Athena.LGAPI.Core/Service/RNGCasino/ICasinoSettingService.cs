using System;
using System.Collections.Generic;
using System.Data;
using Fanex.Athena.LGAPI.Core.Controllers;
using Fanex.Athena.LGAPI.Core.Model.DTO.RNGCasino;
using Fanex.Athena.LGAPI.Core.Model.MetalModel.RNGCasino;

namespace Fanex.Athena.LGAPI.Core.RNGCasino.Service
{
    public interface ICasinoSettingService
    {
        IList<GameKind> GetRngCasinoSettings(int custId, int currencyId, string language);

        bool UpdateSetting(RngCasinoUpdateSettingMetaModel model);
    }
}
