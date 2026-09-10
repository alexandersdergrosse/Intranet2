using Intranet2.Services.ActiveDirectory;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Intranet2.Pages.Ansprechpartner
{
    public class MitarbeiterSucheApiModel : PageModel
    {
        private readonly MitarbeiterService _mitarbeiterService;

        public MitarbeiterSucheApiModel(MitarbeiterService mitarbeiterService)
        {
            _mitarbeiterService = mitarbeiterService;
        }

        // GET /Ansprechpartner/MitarbeiterSucheApi?q=jan&modus=standort
        public IActionResult OnGet(string q, string modus = "standort")
        {
            if (string.IsNullOrWhiteSpace(q) || q.Trim().Length < 2)
                return new JsonResult(new List<object>());

            string suchbegriff = q.Trim().ToLowerInvariant();

            var treffer = _mitarbeiterService
                .GetSuchbareMitarbeiter()
                .Where(m =>
                    (m.Anzeigename?.ToLowerInvariant().Contains(suchbegriff) ?? false) ||
                    (m.FirstName?.ToLowerInvariant().Contains(suchbegriff) ?? false) ||
                    (m.LastName?.ToLowerInvariant().Contains(suchbegriff) ?? false) ||
                    (m.Title?.ToLowerInvariant().Contains(suchbegriff) ?? false) ||
                    (m.Department?.ToLowerInvariant().Contains(suchbegriff) ?? false) ||
                    (m.Niederlassung?.ToLowerInvariant().Contains(suchbegriff) ?? false))
                .Take(20)
                .Select(m => new
                {
                    anzeigename = m.Anzeigename,
                    title = m.Title,
                    department = m.Department,
                    unterabteilung = m.Unterabteilung,
                    niederlassung = m.Niederlassung,
                    telefon = m.TelephoneNumber,
                    mobil = m.Mobile,
                    email = m.Email,
                    samAccountName = m.SamAccountName,
                    istLeitung = m.IstLeitung,
                    initial = m.Anzeigename?.Length > 0 ? m.Anzeigename[0].ToString().ToUpperInvariant() : "?"
                })
                .ToList();

            return new JsonResult(treffer);
        }
    }
}
