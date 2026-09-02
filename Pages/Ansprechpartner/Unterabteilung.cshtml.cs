using Intranet2.Services.ActiveDirectory;
using Intranet2.Services.Fotos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Intranet2.Pages.Ansprechpartner
{
    public class UnterabteilungModel : PageModel
    {
        private readonly MitarbeiterService _mitarbeiterService;
        private readonly MitarbeiterFotoService _fotoService;

        public UnterabteilungModel(MitarbeiterService mitarbeiterService, MitarbeiterFotoService fotoService)
        {
            _mitarbeiterService = mitarbeiterService;
            _fotoService = fotoService;
        }

        public string Abteilung { get; set; } = string.Empty;

        public string Unterabteilung { get; set; } = string.Empty;

        public string HervorgehobenerBenutzername { get; set; } = string.Empty;

        public bool DirekteAbteilungsansicht { get; set; }

        public List<Mitarbeiter> Mitarbeiter { get; set; } = new();

        public Dictionary<string, string?> Fotos { get; set; } = new(StringComparer.OrdinalIgnoreCase);

        public IActionResult OnGet(string abteilung, string? unterabteilung, bool direkt = false, string? person = null)
        {
            // ABTEILUNG PRÜFEN
            if (string.IsNullOrWhiteSpace(abteilung))
            {
                return RedirectToPage("/Ansprechpartner/Abteilungen");
            }

            Abteilung = abteilung.Trim();

            // GESCHÄFTSFÜHRUNG / DIREKTE ABTEILUNG
            bool istGeschaeftsfuehrung = string.Equals(Abteilung, "Geschäftsführung", StringComparison.OrdinalIgnoreCase) || string.Equals(Abteilung, "Geschaeftsfuehrung", StringComparison.OrdinalIgnoreCase);

            DirekteAbteilungsansicht = direkt || istGeschaeftsfuehrung;

            if (DirekteAbteilungsansicht)
            {
                // Keine Description notwendig.
                // Alle Mitarbeiter der Abteilung laden.
                Mitarbeiter = _mitarbeiterService.GetMitarbeiterFuerAbteilung(Abteilung);
            }
            else
            {
                // NORMALE UNTERABTEILUNG
                if (string.IsNullOrWhiteSpace(unterabteilung))
                {
                    return RedirectToPage("/Ansprechpartner/Abteilung",
                        new
                        {
                            abteilung = Abteilung
                        });
                }

                Unterabteilung = unterabteilung.Trim();

                Mitarbeiter = _mitarbeiterService.GetMitarbeiterFuerUnterabteilung(Abteilung, Unterabteilung);

                // GESUCHTE PERSON AN ERSTE STELLE SETZEN
                if (!string.IsNullOrWhiteSpace(person))
                {
                    HervorgehobenerBenutzername = person.Trim();

                    Mitarbeiter = Mitarbeiter
                        .OrderByDescending(m => string.Equals(m.SamAccountName, HervorgehobenerBenutzername, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                }
            }

            // MITARBEITERFOTOS
            foreach (Mitarbeiter mitarbeiter in Mitarbeiter)
            {
                if (string.IsNullOrWhiteSpace(mitarbeiter.SamAccountName))
                {
                    continue;
                }

                Fotos[mitarbeiter.SamAccountName] = _fotoService.GetFotoUrl(mitarbeiter.BereinigterNachname, mitarbeiter.BereinigterVorname);
            }
            return Page();
        }

        // FOTO
        public string? GetFotoUrl(Mitarbeiter mitarbeiter)
        {
            if (string.IsNullOrWhiteSpace(mitarbeiter.SamAccountName))
            {
                return null;
            }

            return Fotos.TryGetValue(mitarbeiter.SamAccountName, out string? fotoUrl) ? fotoUrl : null;
        }

        // INITIAL
        public string GetInitial(Mitarbeiter mitarbeiter)
        {
            string name = mitarbeiter.Anzeigename;

            if (string.IsNullOrWhiteSpace(name))
            {
                return "?";
            }

            return name[0].ToString().ToUpperInvariant();
        }
    }
}