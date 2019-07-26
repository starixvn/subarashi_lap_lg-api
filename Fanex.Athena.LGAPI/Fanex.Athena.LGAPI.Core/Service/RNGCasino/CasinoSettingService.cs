using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fanex.Athena.LGAPI.Core.Controllers;
using Fanex.Athena.LGAPI.Core.Model.DTO.RNGCasino;
using Fanex.Athena.LGAPI.Core.Model.MetalModel.RNGCasino;
using Fanex.Athena.LGAPI.Core.Service.RNGCasino;
using Fanex.Athena.LGAPI.Core.Utility;
using Fanex.Logging;

namespace Fanex.Athena.LGAPI.Core.RNGCasino.Service
{
    public class CasinoSettingService : ICasinoSettingService
    {
        private const string CasinoNoDeposit = "01";

        private readonly ILogger _logger;

        private readonly ICustomerApiWrapper _customerApiWrapper;

        private readonly IBasicInfoApiWrapper _basicInfoApiWrapper;

        public CasinoSettingService(ILogger logger, ICustomerApiWrapper customerApiWrapper,
            IBasicInfoApiWrapper basicInfoApiWrapper)
        {
            _logger = logger;
            _customerApiWrapper = customerApiWrapper;
            _basicInfoApiWrapper = basicInfoApiWrapper;
        }

        public IList<GameKind> GetRngCasinoSettings(int custId, int currencyId, string language)
        {
            IList<GameKind> gameKinds;

            try
            {
                gameKinds = GetGameKind(language);

                foreach (var gameKind in gameKinds)
                {
                    // Get game type base on game kind
                    var gameTypeList = GetGameType(
                        new GetGameTypeMetalModel
                        {
                            GameKind = gameKind.GameKindId,
                            SbCustID = custId,
                            SbExchangeID = currencyId
                        },
                        language);
                    gameKind.GameTypeList = gameTypeList;

                    foreach (var gameType in gameTypeList)
                    {
                        // Get stake level base on game type
                        var stakeLevelList = GetCustStakeLevel(custId, gameType.GameTypeId);
                        gameType.StakeLevelList = stakeLevelList;
                    }
                }
            }
            catch (Exception exception)
            {
                gameKinds = new List<GameKind>();
                _logger.Error(LogMessageHelper.GenerateMessage(exception), exception);
            }

            return gameKinds;
        }

        public bool UpdateSetting(RngCasinoUpdateSettingMetaModel model)
        {

            const int SystemID = 4;
            var table = new DataTable();
            table.Columns.Add("IBCCustID", typeof(int));
            table.Columns.Add("GameTypeID", typeof(int));
            table.Columns.Add("StakeLevelList", typeof(string));
            table.Columns.Add("RoleId", typeof(int));
            table.Columns.Add("FromIP", typeof(string));
            table.Columns.Add("ModifyBy", typeof(int));
            table.Columns.Add("ModifyUserName", typeof(string));
            table.Columns.Add("FromSystem", typeof(int));

            foreach (var item in model.StakeLevelList)
            {
                table.Rows.Add(model.CustId, item.Key, item.Value, 1, model.RequestIP, model.ModifyById, model.ModifyByUsername, SystemID);
            }

            var resultTable = _customerApiWrapper.UpdateCustGameTypeStakeLevel(table);
            return resultTable.Rows.Count == 0;
        }

        private IList<GameKind> GetGameKind(string language)
        {
            var gameKindNameColumn = CasinoHelper.GetColumnNameByLanguage("CodeName", language);
            var gameKindTable = _basicInfoApiWrapper.GetGameKind(CasinoNoDeposit);
            var gameKindList = new List<GameKind>();

            foreach (DataRow row in gameKindTable.Rows)
            {
                var gameKind = new GameKind
                {
                    GameKindId = Convert.ToInt32(row["CodeId"] ?? 0),
                    GameKindName = row[gameKindNameColumn].ToString()
                };

                gameKindList.Add(gameKind);
            }

            return gameKindList;
        }

        private IList<GameType> GetGameType(GetGameTypeMetalModel input, string language)
        {
            var gameTypeName = CasinoHelper.GetColumnNameByLanguage("GameTypeName", language);
            var gameTypeTable = _basicInfoApiWrapper.GetGameType(input);

            var gameTypeList = new List<GameType>();

            foreach (DataRow row in gameTypeTable.Rows)
            {
                var gameType = new GameType
                {
                    GameTypeId = Convert.ToInt32(row["GameTypeId"] ?? 0),
                    GameTypeName = row[gameTypeName].ToString()
                };

                gameTypeList.Add(gameType);
            }

            return gameTypeList.OrderBy(m => m.GameTypeName).ToList();
        }

        private IList<StakeLevel> GetCustStakeLevel(int custId, int gameTypeId)
        {
            var stakeLevelTable = _customerApiWrapper.GetCustStakeLevel(custId, gameTypeId);

            var stakeLevelList = new List<StakeLevel>();

            foreach (DataRow row in stakeLevelTable.Rows)
            {
                var stakeLevel = new StakeLevel
                {
                    StakeLevelId = Convert.ToInt32(row["StakeLevelID"] ?? 0),
                    StakeLevelName = row["StakeLevelName"].ToString(),
                    IsCheck = Convert.ToBoolean(row["IsChecked"] ?? 0)
                };

                stakeLevelList.Add(stakeLevel);
            }

            return stakeLevelList;
        }
    }
}
