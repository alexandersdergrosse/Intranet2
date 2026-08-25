using System.ComponentModel.DataAnnotations;
using Intranet2.Datenbank.Data;
using Intranet2.Datenbank.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Intranet2.Pages.Marktplatz
{
    [Authorize]
    public class NeuModel : PageModel
    {
        private readonly DataContext _context;
        private readonly IWebHostEnvironment _environment;

        public NeuModel(DataContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        [BindProperty]
        public MarktplatzFormular Formular { get; set; } = new();

        [BindProperty]
        public IFormFile? Bild { get; set; }

        public string[] Kategorien => MarktplatzKategorien.Alle;

        public void OnGet() { }
        public async Task<IActionResult> OnPostAsync()
        {
            // KATEGORIE PRÜFEN
            if (!MarktplatzKategorien.Alle.Contains(Formular.Kategorie))
            {
                ModelState.AddModelError("Formular.Kategorie", "Bitte eine gültige Kategorie auswählen.");
            }

            // BILD PRÜFEN
            if (Bild != null)
            {
                string endung = Path.GetExtension(Bild.FileName).ToLowerInvariant();


                string[] erlaubteEndungen = { ".jpg", ".jpeg", ".png", ".webp" };


                if (!erlaubteEndungen.Contains(endung))
                {
                    ModelState.AddModelError("Bild", "Erlaubt sind JPG, JPEG, PNG und WebP.");
                }

                if (Bild.Length > 5 * 1024 * 1024)
                {
                    ModelState.AddModelError("Bild", "Das Bild darf maximal 5 MB groß sein.");
                }
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            // AKTUELLEN BENUTZER LADEN
            string? windowsBenutzername = User.Identity?.Name;

            if (string.IsNullOrWhiteSpace(windowsBenutzername))
            {
                return Forbid();
            }

            Benutzer? benutzer = await _context.Benutzer.FirstOrDefaultAsync(b => b.WindowsBenutzername == windowsBenutzername);


            if (benutzer == null)
            {
                return Forbid();
            }

            // BILD SPEICHERN
            string? bildPfad = null;

            if (Bild != null)
            {
                string endung = Path.GetExtension(Bild.FileName).ToLowerInvariant();

                string dateiname = $"{Guid.NewGuid()}{endung}";

                string ordner = Path.Combine(_environment.WebRootPath, "Images", "Marktplatz");

                Directory.CreateDirectory(ordner);

                string kompletterPfad = Path.Combine(ordner, dateiname);

                await using FileStream stream = new FileStream(kompletterPfad, FileMode.Create);

                await Bild.CopyToAsync(stream);


                bildPfad = $"/Images/Marktplatz/{dateiname}";
            }

            // BEITRAG ERSTELLEN
            MarktplatzBeitrag beitrag = new MarktplatzBeitrag
                {
                    Titel = Formular.Titel.Trim(),

                    Kategorie = Formular.Kategorie,

                    Beschreibung = Formular.Beschreibung.Trim(),

                    Preis = Formular.Preis,

                    BildPfad = bildPfad,

                    ErstelltAm = DateTime.Now,

                    BenutzerId = benutzer.Id
                };

            _context.MarktplatzBeitraege.Add(beitrag);

            await _context.SaveChangesAsync();

            return RedirectToPage("/Marktplatz/Details", new { id = beitrag.Id });
        }

        public class MarktplatzFormular
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