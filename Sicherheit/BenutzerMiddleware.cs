using Intranet2.Datenbank.Data;
using Intranet2.Datenbank.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Intranet2.Sicherheit
{
    public class BenutzerMiddleware
    {
        private readonly RequestDelegate _next;

        private static readonly string[] _statischePfade =
            ["/css", "/js", "/lib", "/Images", "/uploads", "/favicon", "/mitarbeiterfotos"];

        public BenutzerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, DataContext db)
        {
            string path = context.Request.Path.Value ?? "";

            if (_statischePfade.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
            {
                await _next(context);
                return;
            }

            if (context.User.Identity?.IsAuthenticated == true)
            {
                string? windowsBenutzername = context.User.Identity.Name;

                if (!string.IsNullOrWhiteSpace(windowsBenutzername))
                {
                    await VerarbeiteBenutzerAsync(context, db, windowsBenutzername);
                }
            }

            await _next(context);
        }

        private static async Task VerarbeiteBenutzerAsync(HttpContext context, DataContext db, string windowsBenutzername)
        {
            Benutzer? benutzer = await db.Benutzer.FirstOrDefaultAsync(b => b.WindowsBenutzername == windowsBenutzername);

            if (benutzer == null)
            {
                benutzer = await LegeBenutzerAnAsync(db, windowsBenutzername);
            }

            if (benutzer == null)
            {
                return;
            }

            // Neue Marktplatz-Beiträge zählen
            DateTime letzterBesuch = benutzer.LetzterMarktplatzBesuch ?? benutzer.RegisteredAt;
            int neueBeitraege = await db.MarktplatzBeitraege.CountAsync(m => m.ErstelltAm > letzterBesuch && m.BenutzerId != benutzer.Id);
            context.Items["NeueMarktplatzBeitraege"] = neueBeitraege;

            // Gesperrte Benutzer blockieren
            if (!benutzer.IstAktiv)
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsync("Ihr Benutzerkonto ist für das Intranet deaktiviert.");
                return;
            }

            // Rollen als Claims hinzufügen
            var claims = new List<Claim>
            {
                new(ClaimTypes.Role, Rollen.Benutzer)
            };

            if (benutzer.Rolle == Rollen.Admin) claims.Add(new Claim(ClaimTypes.Role, Rollen.Admin));
            if (benutzer.Rolle == Rollen.Redaktion) claims.Add(new Claim(ClaimTypes.Role, Rollen.Redaktion));

            context.User.AddIdentity(new ClaimsIdentity(claims, "IntranetRollen"));
        }

        private static async Task<Benutzer?> LegeBenutzerAnAsync(DataContext db, string windowsBenutzername)
        {
            string name = windowsBenutzername.Contains('\\') ? windowsBenutzername.Split('\\').Last() : windowsBenutzername;

            var benutzer = new Benutzer
            {
                WindowsBenutzername = windowsBenutzername,
                Name = name,
                Email = null,
                Rolle = Rollen.Benutzer,
                IstAktiv = true,
                RegisteredAt = DateTime.UtcNow,
                LetzterMarktplatzBesuch = DateTime.UtcNow,
            };

            db.Benutzer.Add(benutzer);

            try
            {
                await db.SaveChangesAsync();

                db.BenutzerProtokolle.Add(new BenutzerProtokoll
                {
                    BenutzerId = benutzer.Id,
                    BenutzerName = benutzer.Name,
                    WindowsBenutzername = benutzer.WindowsBenutzername,
                    Aktion = "Neu angelegt",
                    Feld = null,
                    AlterWert = null,
                    NeuerWert = $"Rolle: {benutzer.Rolle}; Status: Aktiv",
                    AusgefuehrtVon = "System (erste Anmeldung)",
                    Zeitpunkt = DateTime.UtcNow
                });

                await db.SaveChangesAsync();
                return benutzer;
            }
            catch (DbUpdateException)
            {
                // Anderer Request war schneller → Benutzer nochmal laden
                db.ChangeTracker.Clear();
                return await db.Benutzer.FirstOrDefaultAsync(b => b.WindowsBenutzername == windowsBenutzername);
            }
        }
    }
}
