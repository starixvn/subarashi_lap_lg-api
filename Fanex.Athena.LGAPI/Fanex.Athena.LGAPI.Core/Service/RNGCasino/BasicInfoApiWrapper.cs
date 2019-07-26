using System.Data;

using CasinoLibrary.SBAgent;

using Fanex.Athena.LGAPI.Core.Model.MetalModel.RNGCasino;

namespace Fanex.Athena.LGAPI.Core.Service.RNGCasino
{
    public class BasicInfoApiWrapper : IBasicInfoApiWrapper
    {
        private readonly BasicInfo _basicInfoApi;

        public BasicInfoApiWrapper()
        {
            _basicInfoApi = new BasicInfo();
        }

        public DataTable GetGameKind(string casinoNo)
        {
            return new ApiMethodOperationHandler<DataTable>().Handle(
                casinoNo,
                (obj) => _basicInfoApi.GetGameKind(obj));
        }

        public DataTable GetGameType(GetGameTypeMetalModel input)
        {
            return new ApiMethodOperationHandler<DataTable>().Handle(
                input,
                (obj) => _basicInfoApi.GetGameType(obj.GameKind, obj.SbCustID, obj.SbExchangeID));
        }
    }
}
