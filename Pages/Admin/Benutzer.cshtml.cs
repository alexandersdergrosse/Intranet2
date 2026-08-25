using Intranet2.Datenbank.Data;
using Intranet2.Datenbank.Models;
using Intranet2.Sicherheit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Intranet2.Pages.Admin
{
    [Authorize(Policy = Berechtigungen.BenutzerVerwalten)]
    public class BenutzerModel : PageModel
    {
        private readonly DataContext _context;

        public BenutzerModel(DataContext context)
        {
            _context = context;
        }

        public List<Benutzer> BenutzerListe { get; set; } = new();

        public List<BenutzerProtokoll> ProtokollListe { get; set; } = new();

        [TempData]
        public string? Meldung { get; set; }

        // BENUTZER LADEN
        public async Task OnGetAsync()
        {
            await LadeDatenAsync();
        }

        // ROLLE ÄNDERN
        public async Task<IActionResult> OnPostRolleAsync(int id, string rolle)
        {
            Benutzer? benutzer = await _context.Benutzer.FindAsync(id);

            if (benutzer == null)
            {
                return NotFound();
            }

            // Nur bekannte Rollen erlauben
            if (rolle != Rollen.Benutzer && rolle != Rollen.Redaktion && rolle != Rollen.Admin)
            {
                return BadRequest();
            }


            // Rolle wurde gar nicht geändert
            if (benutzer.Rolle == rolle)
            {
                Meldung = $"Die Rolle von {benutzer.Name} ist bereits {rolle}.";

                return RedirectToPage();
            }


            // Eigene Rolle darf nicht geändert werden
            if (IstAktuellerBenutzer(benutzer))
            {
                Meldung = "Du kannst deine eigene Rolle nicht ändern.";

                return RedirectToPage();
            }

            string alteRolle = benutzer.Rolle;


            benutzer.Rolle = rolle;

            ProtokolliereAenderung(benutzer, "Rolle geändert", "Rolle", alteRolle, rolle);

            await _context.SaveChangesAsync();

            Meldung = $"Die Rolle von {benutzer.Name} wurde auf {rolle} geändert.";

            return RedirectToPage();
        }

        // AKTIV / INAKTIV
        public async Task<IActionResult> OnPostStatusAsync(int id)
        {
            Benutzer? benutzer = await _context.Benutzer.FindAsync(id);

            if (benutzer == null)
            {
                return NotFound();
            }


            // Admin darf sich nicht selbst sperren
            if (IstAktuellerBenutzer(benutzer))
            {
                Meldung = "Du kannst dein eigenes Benutzerkonto nicht deaktivieren.";

                return RedirectToPage();
            }

            string alterStatus = benutzer.IstAktiv ? "Aktiv" : "Deaktiviert";

            benutzer.IstAktiv = !benutzer.IstAktiv;

            string neuerStatus = benutzer.IstAktiv ? "Aktiv" : "Deaktiviert";

            ProtokolliereAenderung(benutzer, "Status geändert", "Status", alterStatus, neuerStatus);

            await _context.SaveChangesAsync();

            Meldung = benutzer.IstAktiv ? $"{benutzer.Name} wurde aktiviert." : $"{benutzer.Name} wurde deaktiviert.";

            return RedirectToPage();
        }

        // PRÜFEN, OB ES DER AKTUELLE WINDOWS-BENUTZER IST
        private bool IstAktuellerBenutzer(Benutzer benutzer)
        {
            string windowsName = User.Identity?.Name ?? string.Empty;

            return string.Equals(benutzer.WindowsBenutzername, windowsName, StringComparison.OrdinalIgnoreCase);
        }

        // BENUTZER + PROTOKOLL LADEN
        private async Task LadeDatenAsync()
        {
            BenutzerListe = await _context.Benutzer.AsNoTracking().OrderBy(b => b.Name).ToListAsync();

            ProtokollListe = await _context.BenutzerProtokolle.AsNoTracking().OrderByDescending(p => p.Zeitpunkt)
                    // Zunächst nur die letzten 100 anzeigen
                    .Take(100).ToListAsync();
        }

        // BENUTZERÄNDERUNG PROTOKOLLIEREN
        private void ProtokolliereAenderung(Benutzer benutzer, string aktion, string feld, string? alterWert, string? neuerWert)
        {
            string ausgefuehrtVon = User.Identity?.Name ?? "Unbekannt";


            BenutzerProtokoll protokoll = new BenutzerProtokoll
                {
                    BenutzerId = benutzer.Id,

                    BenutzerName = benutzer.Name,

                    WindowsBenutzername = benutzer.WindowsBenutzername,

                    Aktion = aktion,

                    Feld = feld,

                    AlterWert = alterWert,

                    NeuerWert = neuerWert,

                    AusgefuehrtVon = ausgefuehrtVon,

                    Zeitpunkt = DateTime.UtcNow
                };

            _context.BenutzerProtokolle.Add(protokoll);
        }
    }
}