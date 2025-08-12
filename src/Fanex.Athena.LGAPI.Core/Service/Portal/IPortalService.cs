namespace Fanex.Athena.LGAPI.Core.Service
{
    using System.Net.Http;
    using System.Threading.Tasks;

    public interface IPortalService
    {
        Task<HttpResponseMessage> GetSpecialLeagues(string siteName);

        Task<HttpResponseMessage> GetLimitLeagues(string siteName, int sportType, int leagueGroupId, string langCode);

        Task<HttpResponseMessage> GetLeagueGroups(int sportType, string langCode);
    }
}