using VeragWebApp.Repos;
using VeragWebApp.Repos.Models;
using VeragWebApp.Service;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace VeragWebApp.Container
{
    public class RefreshHandler : IRefreshHandler
    {
        private readonly VeragDB _context;

        public RefreshHandler(VeragDB context)
        {
            _context = context;
        }

        public async Task<string> GenerateToken(string username, string deviceId)
        {
            // Generiere einen sicheren Refresh-Token
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
            }
            string refreshToken = Convert.ToBase64String(randomNumber);

            // In der Datenbank ablegen
            var newRefreshToken = new TblRefreshtoken
            {
                DeviceId = deviceId,
                UserId = username,
                RefreshToken = refreshToken,
                Created = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddDays(30),
                Revoked = null,
          
            };

            await _context.TblRefreshtokens.AddAsync(newRefreshToken);
            await _context.SaveChangesAsync();

            return refreshToken;
        }

        public async Task<TblRefreshtoken> GetRefreshTokenByDeviceId(string deviceId)
        {
            // Verwenden Sie die direkte Bedingung anstelle von IsActive
            return await _context.TblRefreshtokens
                .FirstOrDefaultAsync(r => r.DeviceId == deviceId && r.Revoked == null && r.Expires > DateTime.UtcNow);
        }

    }
}
