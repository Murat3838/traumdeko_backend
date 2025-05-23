using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using VeragWebApp.Repos;
using VeragWebApp.Repos.Models;

namespace VeragTvApp.server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LogoutController : ControllerBase
    {
        private readonly VeragDB _context;

        public LogoutController(VeragDB context)
        {
            _context = context;
        }

        // Dieser Endpunkt erfordert, dass der Benutzer authentifiziert ist.
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            // Prüfen, ob beide Cookies vorhanden sind.
            if (Request.Cookies.TryGetValue("refreshToken", out var refreshToken) &&
                Request.Cookies.TryGetValue("deviceid", out var deviceId))
            {
                // Suche den entsprechenden Refresh-Token-Eintrag in der Datenbank.
                var tokenEntry = await _context.TblRefreshtokens
                    .FirstOrDefaultAsync(t => t.RefreshToken == refreshToken && t.DeviceId == deviceId);

                // Falls ein Eintrag gefunden wurde, markiere ihn als widerrufen.
                if (tokenEntry != null)
                {
                    tokenEntry.Revoked = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }

                // Lösche die Cookies beim Client.
                Response.Cookies.Delete("accessToken", GetCookieOptions());
                Response.Cookies.Delete("refreshToken", GetCookieOptions());
                Response.Cookies.Delete("deviceid", GetCookieOptions());

                return Ok(new { message = "Logout erfolgreich. Refresh-Token wurde widerrufen." });
            }
            else
            {
                // Falls nicht beide Cookies vorhanden sind, lösche alle eventuell vorhandenen Cookies.
                Response.Cookies.Delete("accessToken", GetCookieOptions());
                Response.Cookies.Delete("refreshToken", GetCookieOptions());
                Response.Cookies.Delete("deviceid", GetCookieOptions());

                return Ok(new { message = "Logout erfolgreich. Keine Token gefunden." });
            }
        }

        // Hilfsmethode zur Erzeugung der CookieOptions, sodass beim Löschen dieselben Optionen verwendet werden.
        private CookieOptions GetCookieOptions()
        {
            return new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None
            };
        }
    }
}
