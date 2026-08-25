using System.ComponentModel.DataAnnotations;
using Intranet2.Datenbank.Data;
using Intranet2.Datenbank.Models;
using Intranet2.Sicherheit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Intranet2.Pages.Marktplatz
{
    [Authorize]
    public class BearbeitenModel : PageModel
    {
        private readonly DataContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _configuration;

        public BearbeitenModel(DataContext context, IWebHostEnvironment environment, IConfiguration configuration)
        {
            _context = context;
            _environment = environment;
            _configuration = configuration;
        }

        public MarktplatzBeitrag Beitrag { get; set; } = null!;

        [BindProperty]
        public BearbeitenFormular Formular { get; set; } = new();

        [BindProperty]
        public IFormFile? Bild { get; set; }

        [BindProperty]
        public bool BildLoeschen { get; set; }

        public string[] Kategorien => MarktplatzKategorien.Alle;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var beitrag = await _context.MarktplatzBeitraege.Include(m => m.Benutzer).FirstOrDefaultAsync(m => m.Id == id);

            if (beitrag == null) return NotFound();

            // Nur eigener Beitrag oder Admin
            bool darfBearbeiten = User.IsInRole(Rollen.Admin) || string.Equals(beitrag.Benutzer.WindowsBenutzername, User.Identity?.Name, StringComparison.OrdinalIgnoreCase);

            if (!darfBearbeiten) return Forbid();

            Beitrag = beitrag;

            // Formular mit aktuellen Werten befüllen
            Formular.Titel = beitrag.Titel;
            Formular.Kategorie = beitrag.Kategorie;
            Formular.Beschreibung = beitrag.Beschreibung;
            Formular.Preis = beitrag.Preis;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var beitrag = await _context.MarktplatzBeitraege.Include(m => m.Benutzer).FirstOrDefaultAsync(m => m.Id == id);

            if (beitrag == null) return NotFound();

            // Nur eigener Beitrag oder Admin
            bool darfBearbeiten = User.IsInRole(Rollen.Admin) || string.Equals(beitrag.Benutzer.WindowsBenutzername, User.Identity?.Name, StringComparison.OrdinalIgnoreCase);

            if (!darfBearbeiten) return Forbid();

            Beitrag = beitrag;

            // KATEGORIE PRÜFEN
            if (!MarktplatzKategorien.Alle.Contains(Formular.Kategorie)) ModelState.AddModelError("Formular.Kategorie", "Bitte eine gültige Kategorie auswählen.");

            // BILD PRÜFEN
            if (Bild != null)
            {
                string endung = Path.GetExtension(Bild.FileName).ToLowerInvariant();
                string[] erlaubteEndungen = { ".jpg", ".jpeg", ".png", ".webp" };

                if (!erlaubteEndungen.Contains(endung)) ModelState.AddModelError("Bild", "Erlaubt sind JPG, JPEG, PNG und WebP.");

                if (Bild.Length > 5 * 1024 * 1024) ModelState.AddModelError("Bild", "Das Bild darf maximal 5 MB groß sein.");
            }

            if (!ModelState.IsValid) return Page();

            // ALTES BILD LÖSCHEN (wenn gewünscht oder neues hochgeladen)
            if ((BildLoeschen || Bild != null) && !string.IsNullOrWhiteSpace(beitrag.BildPfad))
            {
                string uploadPfad = _configuration["Uploads:Pfad"] ?? Path.Combine(_environment.WebRootPath, "Images", "Marktplatz");
                string dateiname = Path.GetFileName(beitrag.BildPfad);
                string alterPfad = Path.Combine(uploadPfad, dateiname);

                if (System.IO.File.Exists(alterPfad)) System.IO.File.Delete(alterPfad);

                beitrag.BildPfad = null;
            }

            // NEUES BILD SPEICHERN
            if (Bild != null)
            {
                string endung = Path.GetExtension(Bild.FileName).ToLowerInvariant();
                string neuerDateiname = $"{Guid.NewGuid()}{endung}";
                string ordner = _configuration["Uploads:Pfad"] ?? Path.Combine(_environment.WebRootPath, "Images", "Marktplatz");

                Directory.CreateDirectory(ordner);
                string kompletterPfad = Path.Combine(ordner, neuerDateiname);

                await using FileStream stream = new FileStream(kompletterPfad, FileMode.Create);
                await Bild.CopyToAsync(stream);

                beitrag.BildPfad = $"/uploads/{neuerDateiname}";
            }

            // FELDER AKTUALISIEREN
            beitrag.Titel = Formular.Titel.Trim();
            beitrag.Kategorie = Formular.Kategorie;
            beitrag.Beschreibung = Formular.Beschreibung.Trim();
            beitrag.Preis = Formular.Preis;

            await _context.SaveChangesAsync();

            return RedirectToPage("/Marktplatz/Details", new { id = beitrag.Id });
        }

        public class BearbeitenFormular
        {
            [Required(ErrorMessage = "Bitte einen Titel eingeben.")]
            [MaxLength(150)]
            public string Titel { get; set; } = string.Empty;

            [Required(ErrorMessage = "Bitte eine Kategorie auswählen.")]
            public string Kategorie { get; set; } = MarktplatzKategorien.Verkaufen;

            [Required(ErrorMessage = "Bitte eine Beschreibung eingeben.")]
            [MaxLength(3000)]
            public string Beschreibung { get; set; } = string.Empty;

            [Range(0, 1000000, ErrorMessage = "Bitte einen gültigen Preis eingeben.")]
            public decimal? Preis { get; set; }
        }
    }
}
