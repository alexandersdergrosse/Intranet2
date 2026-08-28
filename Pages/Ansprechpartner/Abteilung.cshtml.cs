using Intranet2.Services.ActiveDirectory;
using Intranet2.Services.Fotos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Intranet2.Pages.Ansprechpartner
{
    public class AbteilungModel : PageModel
    {
        private readonly MitarbeiterService _mitarbeiterService;
        private readonly MitarbeiterFotoService _fotoService;

        public AbteilungModel(MitarbeiterService mitarbeiterService, MitarbeiterFotoService fotoService)
        {
            _mitarbeiterService = mitarbeiterService;
            _fotoService = fotoService;
        }

        public string Abteilung { get; set; } = string.Empty;
        public List<Mitarbeiter> Mitarbeiter { get; set; } = new();

        public Dictionary<string, string?> Fotos { get; set; } = new();

        public IActionResult OnGet(string abteilung)
        {
            if (string.IsNullOrWhiteSpace(abteilung)) return RedirectToPage("/Ansprechpartner/Abteilungen");

            Abteilung = abteilung;
            Mitarbeiter = _mitarbeiterService.GetMitarbeiterFuerAbteilung(abteilung);

            foreach (var m in Mitarbeiter)
            {
                string vorname = m.FirstName?.Trim() ?? string.Empty;
                string nachname = m.LastName?.Trim() ?? string.Empty;
                Fotos[m.SamAccountName] = _fotoService.GetFotoUrl(nachname, vorname);
            }

            return Page();
        }
    }
}
