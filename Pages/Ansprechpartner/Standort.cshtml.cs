using Intranet2.Services.ActiveDirectory;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Intranet2.Pages.Ansprechpartner
{
    public class StandortModel : PageModel
    {
        private readonly MitarbeiterService _mitarbeiterService;

        public StandortModel(MitarbeiterService mitarbeiterService)
        {
            _mitarbeiterService = mitarbeiterService;
        }

        public string Niederlassung { get; set; } = string.Empty;

        public List<Mitarbeiter> Mitarbeiter { get; set; } = new();

        public IActionResult OnGet(string niederlassung)
        {
            if (string.IsNullOrWhiteSpace(niederlassung))
            {
                return RedirectToPage("/Ansprechpartner/Ansprechpartner");
            }

            Niederlassung = niederlassung;

            Mitarbeiter = _mitarbeiterService.GetMitarbeiterFuerNiederlassung(niederlassung);

            return Page();
        }
    }
}