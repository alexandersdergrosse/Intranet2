using Intranet2.Services.ActiveDirectory;
using Intranet2.Services.Fotos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Intranet2.Pages.Ansprechpartner
{
    public class StandortModel : PageModel
    {
        private readonly MitarbeiterService _mitarbeiterService;
        private readonly MitarbeiterFotoService _fotoService;

        public StandortModel(MitarbeiterService mitarbeiterService, MitarbeiterFotoService fotoService)
        {
            _mitarbeiterService = mitarbeiterService;
            _fotoService = fotoService;
        }

        public string Niederlassung { get; set; } = string.Empty;
        public string HervorgehobenerBenutzername { get; set; } = string.Empty;
        public List<Mitarbeiter> Mitarbeiter { get; set; } = new();

        public Dictionary<string, string?> Fotos { get; set; } = new();

        public IActionResult OnGet(string niederlassung, string? person = null)
        {
            if (string.IsNullOrWhiteSpace(niederlassung)) return RedirectToPage("/Ansprechpartner/Ansprechpartner");

            Niederlassung = niederlassung;
            Mitarbeiter = _mitarbeiterService.GetMitarbeiterFuerNiederlassung(niederlassung);

            // GESUCHTE PERSON AN ERSTE STELLE SETZEN
            if (!string.IsNullOrWhiteSpace(person))
            {
                HervorgehobenerBenutzername = person.Trim();

                Mitarbeiter = Mitarbeiter
                    .OrderByDescending(m => string.Equals(m.SamAccountName, HervorgehobenerBenutzername, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            foreach (var m in Mitarbeiter)
            {
                Fotos[m.SamAccountName] = _fotoService.GetFotoUrl(m.BereinigterNachname, m.BereinigterVorname);
            }

            return Page();
        }
    }
}
