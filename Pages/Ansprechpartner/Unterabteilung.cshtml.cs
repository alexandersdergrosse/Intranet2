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


        public UnterabteilungModel(
            MitarbeiterService mitarbeiterService,
            MitarbeiterFotoService fotoService)
        {
            _mitarbeiterService = mitarbeiterService;

            _fotoService = fotoService;
        }


        public string Abteilung { get; set; }
            = string.Empty;


        public string Unterabteilung { get; set; }
            = string.Empty;


        public List<Mitarbeiter> Mitarbeiter { get; set; }
            = new();


        public Dictionary<string, string?> Fotos { get; set; }
            = new();


        public IActionResult OnGet(
            string abteilung,
            string unterabteilung)
        {
            if (string.IsNullOrWhiteSpace(abteilung))
            {
                return RedirectToPage(
                    "/Ansprechpartner/Abteilungen");
            }


            if (string.IsNullOrWhiteSpace(unterabteilung))
            {
                return RedirectToPage(
                    "/Ansprechpartner/Abteilung",
                    new
                    {
                        abteilung
                    });
            }


            Abteilung = abteilung;

            Unterabteilung = unterabteilung;


            Mitarbeiter =
                _mitarbeiterService
                    .GetMitarbeiterFuerUnterabteilung(
                        abteilung,
                        unterabteilung);


            foreach (Mitarbeiter mitarbeiter in Mitarbeiter)
            {
                string vorname =
                    mitarbeiter.FirstName?.Trim()
                    ?? string.Empty;


                string nachname =
                    mitarbeiter.LastName?.Trim()
                    ?? string.Empty;


                Fotos[mitarbeiter.SamAccountName] =
                    _fotoService.GetFotoUrl(
                        nachname,
                        vorname);
            }


            return Page();
        }
    }
}