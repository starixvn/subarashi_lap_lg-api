using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fanex.Athena.LGAPI.Core.Service.RNGCasino
{
   public interface ICustomerApiWrapper
    {
        DataTable UpdateCustGameTypeStakeLevel(DataTable sourceTable);

        DataTable GetCustStakeLevel(int ibcCustId, int gameTypeId);
    }
}
