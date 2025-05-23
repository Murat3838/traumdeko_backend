
using VeragWebApp.Repos.Models;

namespace VeragWebApp.Service
{
    public interface IRefreshHandler
    {
        Task<string> GenerateToken(string username, string deviceId);
        Task<TblRefreshtoken> GetRefreshTokenByDeviceId(string deviceId);

    }

}
