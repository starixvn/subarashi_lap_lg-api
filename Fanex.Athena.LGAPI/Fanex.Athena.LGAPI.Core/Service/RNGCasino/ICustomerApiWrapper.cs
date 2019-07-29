using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fanex.Athena.LGAPI.Core.Model.MetalModel.RNGCasino;

namespace Fanex.Athena.LGAPI.Core.Service.RNGCasino
{
   public interface ICustomerApiWrapper
    {
        DataTable UpdateCustGameTypeStakeLevel(DataTable sourceTable);

        DataTable GetCustStakeLevel(int ibcCustId, int gameTypeId);

        bool UpdateCustEnableCasino(BetSettingCasinoStatusMetaModel metaModel);

        bool UpdateCustSuspend(CustomerStatusMetaModel metaModel);

        bool UpdateCustClosed(CustomerStatusMetaModel metaModel);

        bool UpdateComm(RNGCasinoUpdateMemberComMetaModel metaModel);
    }
}
