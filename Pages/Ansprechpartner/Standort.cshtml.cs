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
        public List<Mitarbeiter> Mitarbeiter { get; set; } = new();

        public Dictionary<string, string?> Fotos { get; set; } = new();

        public IActionResult OnGet(string niederlassung)
        {
            if (string.IsNullOrWhiteSpace(niederlassung)) return RedirectToPage("/Ansprechpartner/Ansprechpartner");

            Niederlassung = niederlassung;
            Mitarbeiter = _mitarbeiterService.GetMitarbeiterFuerNiederlassung(niederlassung);

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
