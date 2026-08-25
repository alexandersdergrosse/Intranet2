using Intranet2.Services.ActiveDirectory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Intranet2.Pages.MeinBereich
{
    [Authorize]
    public class ProfilModel : PageModel
    {
        private readonly MitarbeiterService _mitarbeiterService;

        public ProfilModel(MitarbeiterService mitarbeiterService)
        {
            _mitarbeiterService = mitarbeiterService;
        }

        public string Anmeldename { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string EmailLink { get; set; } = string.Empty;

        public string Anzeigename { get; set; } = string.Empty;

        public void OnGet()
        {
            // Vollständiger Windows-Anmeldename,
            // z. B. KREUZTRAEGER\schoen
            string windowsBenutzername = User.Identity?.Name ?? string.Empty;

            // Mitarbeiter im Active Directory suchen
            Mitarbeiter? mitarbeiter = _mitarbeiterService.GetMitarbeiterFuerBenutzername(windowsBenutzername);

            // ----------------------------------------------------
            // ANMELDENAME
            // ----------------------------------------------------

            // Aus KREUZTRAEGER\schoen wird nur schoen
            Anmeldename = windowsBenutzername.Contains("\\") ? windowsBenutzername.Split('\\').Last() : windowsBenutzername;

            // ----------------------------------------------------
            // ANGEZEIGTE E-MAIL
            // ----------------------------------------------------
            if (!string.IsNullOrWhiteSpace(Anmeldename))
            {
                Email = $"{Anmeldename}@kreutztraeger.com";
            }

            // ----------------------------------------------------
            // ACTIVE DIRECTORY
            // ----------------------------------------------------
            if (mitarbeiter != null)
            {
                Anzeigename = mitarbeiter.DisplayName;

                // Der Link führt zur tatsächlich im AD
                // hinterlegten E-Mail-Adresse.
                if (!string.IsNullOrWhiteSpace(mitarbeiter.Email))
                {
                    EmailLink = mitarbeiter.Email;
                }
            }

            // Falls im AD keine Mail hinterlegt ist,
            // verwenden wir die angezeigte Adresse.
            if (string.IsNullOrWhiteSpace(EmailLink))
            {
                EmailLink = Email;
            }
        }
    }
}