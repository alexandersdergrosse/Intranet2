using Intranet2.Services.ActiveDirectory;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Intranet2.Pages.Ansprechpartner
{
    public class AbteilungenModel : PageModel
    {
        private readonly MitarbeiterService _mitarbeiterService;

        public AbteilungenModel(MitarbeiterService mitarbeiterService)
        {
            _mitarbeiterService = mitarbeiterService;
        }

        public List<AbteilungGruppe> Abteilungen { get; set; } = new();

        public void OnGet()
        {
            Abteilungen = _mitarbeiterService.GetMitarbeiterNachAbteilung();
        }
    }
}