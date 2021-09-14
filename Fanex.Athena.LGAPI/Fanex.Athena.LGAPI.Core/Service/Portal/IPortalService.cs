using System.Net.Http;
using System.Threading.Tasks;

namespace Fanex.Athena.LGAPI.Core.Service
{
    public interface IPortalService
    {
        Task<HttpResponseMessage> GetSpecialLeagues(string siteName);

        Task<HttpResponseMessage> GetLimitLeagues(string siteName, int sportType, int leagueGroupId, string langCode);

        Task<HttpResponseMessage> GetLeagueGroups(int sportType, string langCode);
    }
}