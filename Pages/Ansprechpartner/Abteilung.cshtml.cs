using Intranet2.Services.ActiveDirectory;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Intranet2.Pages.Ansprechpartner
{
    public class AbteilungModel : PageModel
    {
        private readonly MitarbeiterService _mitarbeiterService;

        public AbteilungModel(MitarbeiterService mitarbeiterService)
        {
            _mitarbeiterService = mitarbeiterService;
        }

        public string Abteilung { get; set; } = string.Empty;

        public List<UnterabteilungGruppe> Unterabteilungen { get; set; } = new();

        public IActionResult OnGet(string abteilung)
        {
            if (string.IsNullOrWhiteSpace(abteilung))
            {
                return RedirectToPage("/Ansprechpartner/Abteilungen");
            }

            Abteilung = abteilung;

            Unterabteilungen = _mitarbeiterService.GetUnterabteilungenFuerAbteilung(abteilung);

            return Page();
        }
    }
}