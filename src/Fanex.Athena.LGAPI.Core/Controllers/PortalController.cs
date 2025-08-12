namespace Fanex.Athena.LGAPI.Core.Controllers
{
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using Attributes;
    using Model.MetalModel;
    using Service;

    [CustomAuth]
    [HandleJsonException]
    public class PortalController : Controller
    {
        private readonly IPortalService _portalService;

        public PortalController(IPortalService portalService)
        {
            _portalService = portalService;
        }

        [HttpPost]
        public async Task<ActionResult> GetSpecialLeagues(GetSpecialLeaguesMetaModel metaModel)
            => await ToJsonResponse(await _portalService.GetSpecialLeagues(metaModel.SiteName));

        [HttpPost]
        public async Task<ActionResult> GetLimitLeagues(GetLimitLeaguesMetaModel metaModel)
            => await ToJsonResponse(await _portalService.GetLimitLeagues(metaModel.SiteName, metaModel.SportType, metaModel.LeagueGroupId, metaModel.LangCode));

        [HttpPost]
        public async Task<ActionResult> GetLeagueGroups(GetLeagueGroupsMetaModel metaModel)
            => await ToJsonResponse(await _portalService.GetLeagueGroups(metaModel.SportType, metaModel.LangCode));

        private async Task<ActionResult> ToJsonResponse(HttpResponseMessage responseMessage)
        {
            const string JsonContentType = "application/json";
            try
            {
                var json = await responseMessage.Content.ReadAsStringAsync();
                return Content(json, JsonContentType);
            }
            finally
            {
                responseMessage.Dispose();
            }
        }
    }
}