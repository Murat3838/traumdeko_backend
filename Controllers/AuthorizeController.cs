// AuthorizeController.cs
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using VeragWebApp.Modal;
using VeragWebApp.Repos;
using VeragWebApp.Repos.Models;
using VeragWebApp.Service;
using VeragWebApp.server.Helper;
using VeragTvApp.server.Services;

namespace VeragWebApp.Controllers
{
     [Route("api/[controller]")]
    [ApiController]
     public class AuthorizeController : ControllerBase
    {
        private readonly VeragDB _context;
        private readonly JwtSettings _jwtSettings;
        private readonly IRefreshHandler _refresh;
 
        public AuthorizeController(
            VeragDB context,
            IOptions<JwtSettings> options,
        IRefreshHandler refresh)
         {
            _context = context;
            _jwtSettings = options.Value;
            _refresh = refresh;
        }

        // Dieser Endpunkt muss auch für nicht authentifizierte Benutzer (z. B. beim Login) verfügbar sein.
        // Deshalb wird hier [AllowAnonymous] verwendet. Anschließend wird aber geprüft, ob der Benutzer Admin ist.
        [HttpPost("GenerateToken")]
        [AllowAnonymous]
        public async Task<IActionResult> GenerateToken([FromBody] UserCred userCred)
        {
            // Suche den Benutzer anhand der übergebenen Zugangsdaten.
            var user = await _context.TblUsers.FirstOrDefaultAsync(
                item => item.Username == userCred.username &&
                        item.Password == userCred.password &&
                        item.isGekuendigt == false);
            
            if (user == null)
                return Unauthorized();

            // Ermittle die Rolle des Benutzers.
            string roleToAssign = await GetUserRoleAsync(user.Username);

          
    

            // 1) Access Token erstellen
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenKey = Encoding.UTF8.GetBytes(_jwtSettings.securitykey);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, roleToAssign)
                }),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                Expires = DateTime.UtcNow.AddMinutes(60),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(tokenKey),
                    SecurityAlgorithms.HmacSha256)
            };

            var accessToken = tokenHandler.CreateToken(tokenDescriptor);
            var finalAccessToken = tokenHandler.WriteToken(accessToken);

            // 2) DeviceId (deviceid) verarbeiten
            string deviceId;
            bool isNewDevice = false;

            if (Request.Cookies.TryGetValue("deviceid", out var existingDeviceId))
            {
                deviceId = existingDeviceId;
            }
            else
            {
                deviceId = Guid.NewGuid().ToString("N");
                isNewDevice = true;
            }

            // 3) Überprüfen, ob ein aktiver Refresh-Token für diese DeviceId existiert
            string refreshTokenString;

            if (!isNewDevice)
            {
                var existingRefreshToken = await _refresh.GetRefreshTokenByDeviceId(deviceId);
                if (existingRefreshToken != null)
                {
                    refreshTokenString = existingRefreshToken.RefreshToken;
                }
                else
                {
                    refreshTokenString = await _refresh.GenerateToken(user.Username, deviceId);
                }
            }
            else
            {
                refreshTokenString = await _refresh.GenerateToken(user.Username, deviceId);
            }

            // 4) Cookies setzen

            // Access-Token Cookie
            HttpContext.Response.Cookies.Append("accessToken", finalAccessToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddMinutes(60)
            });

            // Refresh-Token Cookie
            HttpContext.Response.Cookies.Append("refreshToken", refreshTokenString, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddDays(30)
            });

            // DeviceId Cookie setzen, falls es sich um ein neues Device handelt
            if (isNewDevice)
            {
                HttpContext.Response.Cookies.Append("deviceid", deviceId, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Expires = DateTime.UtcNow.AddYears(10)
                });
            }

            return Ok(new
            {
                TimasId = user.TimasId,
                UserRole = roleToAssign,
                Token = finalAccessToken
            });
        }

        // Dieser Endpunkt setzt voraus, dass der Benutzer bereits authentifiziert ist und über ein gültiges Token verfügt.
        [HttpPost("RefreshToken")]
        public async Task<IActionResult> RefreshToken()
        {
            if (!Request.Cookies.TryGetValue("refreshToken", out var refreshToken) ||
                !Request.Cookies.TryGetValue("deviceid", out var deviceId))
            {
                return Unauthorized("Refresh-Token oder DeviceId im Cookie fehlt.");
            }

            var existingRefresh = await _context.TblRefreshtokens
                .FirstOrDefaultAsync(r => r.RefreshToken == refreshToken && r.DeviceId == deviceId);

            if (existingRefresh == null || !existingRefresh.IsActive)
            {
                return Unauthorized("Refresh-Token ungültig oder abgelaufen.");
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenKey = Encoding.UTF8.GetBytes(_jwtSettings.securitykey);

            var username = existingRefresh.UserId;
            string roleToAssign = await GetUserRoleAsync(username);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, username),
                    new Claim(ClaimTypes.Role, roleToAssign)
                }),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                Expires = DateTime.UtcNow.AddMinutes(15),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(tokenKey),
                    SecurityAlgorithms.HmacSha256)
            };

            var newAccessToken = tokenHandler.CreateToken(tokenDescriptor);
            var finalNewAccessToken = tokenHandler.WriteToken(newAccessToken);

            // Aktualisiere nur das Access-Token im Cookie
            HttpContext.Response.Cookies.Append("accessToken", finalNewAccessToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddMinutes(15)
            });

            return Ok(new
            {
                Token = finalNewAccessToken
            });
        }
 
        // Hilfsmethode zur Ermittlung der Benutzerrolle
        private async Task<string> GetUserRoleAsync(string username)
        {
            var normalizedUsername = username.ToLower();
            var roleEntry = await _context.TblRoles
                .FirstOrDefaultAsync(r => r.username.ToLower() == normalizedUsername);

            if (roleEntry != null)
            {
                return roleEntry.Role;
            }

            return "user";
        }
    }
}
