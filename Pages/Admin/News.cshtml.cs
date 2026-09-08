using System.ComponentModel.DataAnnotations;
using Intranet2.Datenbank.Data;
using Intranet2.Datenbank.Models;
using Intranet2.Sicherheit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using HtmlAgilityPack;

namespace Intranet2.Pages.Admin
{
    [Authorize(Policy = Berechtigungen.NewsVerwalten)]
    public class NewsModel : PageModel
    {
        private readonly DataContext _context;
        private readonly IWebHostEnvironment _environment;

        public IReadOnlyDictionary<string, string> Kategorien
        {
            get
            {
                return NewsKategorien.Alle;
            }
        }

        public NewsModel(DataContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // NEWS-LISTE
        public List<NewsBeitrag> NewsListe { get; set; } = new();

        // FORMULAR
        [BindProperty]
        public NewsFormular Formular { get; set; } = new();

        [TempData]
        public string? Meldung { get; set; }

        // SEITE LADEN
        public async Task<IActionResult> OnGetAsync(int? bearbeitenId)
        {
            await LadeNewsAsync();

            // NEUE NEWS
            if (!bearbeitenId.HasValue)
            {
                Formular.VeroeffentlichtAm = DateTime.Now;

                Formular.IstVeroeffentlicht = true;

                return Page();
            }

            // BESTEHENDE NEWS LADEN
            NewsBeitrag? news = await _context.NewsBeitraege.FindAsync(bearbeitenId.Value);

            if (news == null)
            {
                return NotFound();
            }

            Formular = new NewsFormular
            {
                Id = news.Id,
                Titel = news.Titel,
                Kurztext = news.Kurztext,
                Inhalt = news.Inhalt,
                Kategorie = news.Kategorie,
                VeroeffentlichtAm = news.VeroeffentlichtAm,
                IstVeroeffentlicht = news.IstVeroeffentlicht,
                IstKurzmeldung = news.IstKurzmeldung,
                AktuellesBildPfad = news.BildPfad
            };

            return Page();
        }

        // NEWS SPEICHERN
        public async Task<IActionResult> OnPostSpeichernAsync()
        {
            // Bild zuerst prüfen
            if (Formular.BildDatei != null)
            {
                PruefeBild(Formular.BildDatei);
            }

            // NEWS-INHALT BEREINIGEN
            Formular.Inhalt = BereinigeHtml(Formular.Inhalt);

            // Nur normale News brauchen einen ausführlichen Inhalt.
            // Bei Kurzmeldungen darf der Editor leer bleiben.
            if (!Formular.IstKurzmeldung && !HatSichtbarenInhalt(Formular.Inhalt))
            {
                ModelState.AddModelError("Formular.Inhalt", "Bitte einen Inhalt eingeben.");
            }

            // KATEGORIE PRÜFEN
            if (!NewsKategorien.IstGueltig(Formular.Kategorie))
            {
                ModelState.AddModelError("Formular.Kategorie", "Bitte eine gültige Kategorie auswählen.");
            }

            if (!ModelState.IsValid)
            {
                await LadeNewsAsync();

                return Page();
            }

            // NEUE NEWS
            if (Formular.Id == 0)
            {
                string? bildPfad = null;

                if (Formular.BildDatei != null)
                {
                    bildPfad = await SpeichereBildAsync(Formular.BildDatei);
                }


                NewsBeitrag news = new NewsBeitrag
                {
                    Titel = Formular.Titel.Trim(),

                    Kurztext = Formular.Kurztext.Trim(),

                    Inhalt = Formular.Inhalt,

                    Kategorie = Formular.Kategorie.Trim(),

                    KategorieFarbe = NewsKategorien.FarbeFuer(Formular.Kategorie),

                    BildPfad = bildPfad,

                    VeroeffentlichtAm = Formular.VeroeffentlichtAm,

                    IstVeroeffentlicht = Formular.IstVeroeffentlicht,

                    IstKurzmeldung = Formular.IstKurzmeldung,

                    ErstelltAm = DateTime.UtcNow,

                    ErstelltVon = User.Identity?.Name
                };

                _context.NewsBeitraege.Add(news);

                await _context.SaveChangesAsync();

                Meldung = $"Die News „{news.Titel}“ wurde erstellt.";
            }

            // BESTEHENDE NEWS BEARBEITEN
            else
            {
                NewsBeitrag? news = await _context.NewsBeitraege.FindAsync(Formular.Id);

                if (news == null)
                {
                    return NotFound();
                }

                string? alterBildPfad = news.BildPfad;

                string? neuerBildPfad = null;

                // NEUES BILD HOCHGELADEN
                if (Formular.BildDatei != null)
                {
                    neuerBildPfad = await SpeichereBildAsync(Formular.BildDatei);

                    news.BildPfad = neuerBildPfad;
                }

                // BILD ENTFERNEN
                else if (Formular.BildEntfernen)
                {
                    news.BildPfad = null;
                }

                // NEWS-DATEN AKTUALISIEREN
                news.Titel = Formular.Titel.Trim();

                news.Kurztext = Formular.Kurztext.Trim();

                news.Inhalt = Formular.Inhalt;

                news.Kategorie = Formular.Kategorie.Trim();

                news.KategorieFarbe = NewsKategorien.FarbeFuer(Formular.Kategorie);

                news.VeroeffentlichtAm = Formular.VeroeffentlichtAm;

                news.IstVeroeffentlicht = Formular.IstVeroeffentlicht;

                news.IstKurzmeldung = Formular.IstKurzmeldung;

                news.GeaendertAm = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // ALTES BILD ERST NACH ERFOLGREICHEM
                // SPEICHERN LÖSCHEN
                if (!string.IsNullOrWhiteSpace(alterBildPfad))
                {
                    if (neuerBildPfad != null || Formular.BildEntfernen)
                    {
                        LoescheBild(alterBildPfad);
                    }
                }

                Meldung = $"Die News „{news.Titel}“ wurde gespeichert.";
            }

            return RedirectToPage();
        }

        // NEWS LÖSCHEN
        public async Task<IActionResult> OnPostLoeschenAsync(int id)
        {
            NewsBeitrag? news = await _context.NewsBeitraege.FindAsync(id);

            if (news == null)
            {
                return NotFound();
            }

            string titel = news.Titel;

            string? bildPfad = news.BildPfad;

            _context.NewsBeitraege.Remove(news);

            await _context.SaveChangesAsync();

            // Zugehöriges Bild ebenfalls löschen
            if (!string.IsNullOrWhiteSpace(bildPfad))
            {
                LoescheBild(bildPfad);
            }

            Meldung = $"Die News „{titel}“ wurde gelöscht.";

            return RedirectToPage();
        }

        // NEWS AUS DATENBANK LADEN
        private async Task LadeNewsAsync()
        {
            NewsListe = await _context.NewsBeitraege.AsNoTracking().OrderByDescending(n => n.VeroeffentlichtAm).ToListAsync();
        }

        // BILD PRÜFEN
        private void PruefeBild(IFormFile bild)
        {
            const long maximaleDateigroesse = 5 * 1024 * 1024;

            if (bild.Length == 0)
            {
                ModelState.AddModelError("Formular.BildDatei", "Die ausgewählte Datei ist leer.");

                return;
            }

            if (bild.Length > maximaleDateigroesse)
            {
                ModelState.AddModelError("Formular.BildDatei", "Das Bild darf maximal 5 MB groß sein.");
            }

            string dateiendung = Path.GetExtension(bild.FileName).ToLowerInvariant();

            string[] erlaubteDateiendungen =
            {
                ".jpg",
                ".jpeg",
                ".png",
                ".webp"
            };

            if (!erlaubteDateiendungen.Contains(dateiendung))
            {
                ModelState.AddModelError("Formular.BildDatei", "Erlaubt sind nur JPG, JPEG, PNG und WebP.");
            }
        }

        // BILD SPEICHERN
        private async Task<string> SpeichereBildAsync(IFormFile bild)
        {
            string bilderOrdner = Path.Combine(_environment.WebRootPath, "Images", "News");

            Directory.CreateDirectory(bilderOrdner);

            string dateiendung = Path.GetExtension(bild.FileName).ToLowerInvariant();


            string dateiname = $"{Guid.NewGuid()}{dateiendung}";


            string dateiPfad = Path.Combine(bilderOrdner, dateiname);

            await using FileStream stream = new FileStream(dateiPfad, FileMode.Create);

            await bild.CopyToAsync(stream);

            return $"/Images/News/{dateiname}";
        }

        // BILD LÖSCHEN
        private void LoescheBild(string bildPfad)
        {
            // Sicherheitsprüfung:
            // Nur Bilder aus unserem News-Ordner löschen.
            if (!bildPfad.StartsWith("/Images/News/", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            string dateiname = Path.GetFileName(bildPfad);

            if (string.IsNullOrWhiteSpace(dateiname))
            {
                return;
            }

            string dateiPfad = Path.Combine(_environment.WebRootPath, "Images", "News", dateiname);

            if (System.IO.File.Exists(dateiPfad))
            {
                System.IO.File.Delete(dateiPfad);
            }
        }

        // ERLAUBTE HTML-TAGS FÜR NEWS
        private static readonly HashSet<string> ErlaubteHtmlTags = new(StringComparer.OrdinalIgnoreCase)
            {
                "p", 
                "div", 
                "br", 
                
                "strong", "b", 
                
                "em", 
                "i", 
                
                "u", 
                
                "h2", 
                "h3", 
                "h4", 
                
                "ul", 
                "ol", 
                "li", 
                
                "blockquote", 
                
                "a"
            };

        // HTML BEREINIGEN
        private static string BereinigeHtml(string? html)
        {
            if (string.IsNullOrWhiteSpace(html))
            {
                return string.Empty;
            }

            HtmlDocument dokument = new HtmlDocument();

            dokument.LoadHtml(html);

            List<HtmlNode> elemente = dokument.DocumentNode.Descendants().Where(n => n.NodeType == HtmlNodeType.Element).ToList();

            foreach (HtmlNode element in elemente)
            {
                string tag = element.Name.ToLowerInvariant();

                 // GEFÄHRLICHE ELEMENTE KOMPLETT ENTFERNEN
                if (tag == "script" || tag == "style" || tag == "iframe" || tag == "object" || tag == "embed")
                {
                    element.Remove();

                    continue;
                }

                // NICHT ERLAUBTE TAGS ENTFERNEN, INHALT ABER BEHALTEN
                if (!ErlaubteHtmlTags.Contains(tag))
                {
                    HtmlNode? parent = element.ParentNode;

                    if (parent != null)
                    {
                        foreach (HtmlNode child in element.ChildNodes.ToList())
                        {
                            parent.InsertBefore(child, element);
                        }

                        parent.RemoveChild(element);
                    }

                    continue;
                }

                // ATTRIBUTE BEREINIGEN
                foreach (HtmlAttribute attribut in element.Attributes.ToList())
                {
                    // Nur bei Links darf href bestehen bleiben
                    if (tag == "a" && attribut.Name.Equals("href", StringComparison.OrdinalIgnoreCase))
                    {
                        string href = attribut.Value.Trim();

                        bool erlaubt = href.StartsWith("https://", StringComparison.OrdinalIgnoreCase) || href.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || href.StartsWith("mailto:", 
                            StringComparison.OrdinalIgnoreCase) || href.StartsWith("/", StringComparison.Ordinal);

                        if (!erlaubt)
                        {
                            element.Attributes.Remove(attribut);
                        }

                        continue;
                    }

                    // Alle anderen Attribute entfernen
                    element.Attributes.Remove(attribut);
                }
            }

            return dokument.DocumentNode.InnerHtml.Trim();
        }

        // PRÜFEN, OB SICHTBARER TEXT VORHANDEN IST
        private static bool HatSichtbarenInhalt(string html)
        {
            if (string.IsNullOrWhiteSpace(html))
            {
                return false;
            }

            HtmlDocument dokument = new HtmlDocument();

            dokument.LoadHtml(html);

            string text = HtmlEntity.DeEntitize(dokument.DocumentNode.InnerText).Replace('\u00A0', ' ').Trim();

            return !string.IsNullOrWhiteSpace(text);
        }

        // FORMULAR-MODELL
        public class NewsFormular
        {
            public int Id { get; set; }

            [Required(ErrorMessage = "Bitte einen Titel eingeben.")]
            [MaxLength(200)]
            public string Titel { get; set; } = string.Empty;

            [Required(ErrorMessage = "Bitte einen Kurztext eingeben.")]
            [MaxLength(500)]
            public string Kurztext { get; set; } = string.Empty;

            public string Inhalt { get; set; } = string.Empty;

            [Required(ErrorMessage = "Bitte eine Kategorie eingeben.")]
            [MaxLength(100)]
            public string Kategorie { get; set; } = string.Empty;

            [Required]
            public DateTime VeroeffentlichtAm { get; set; } = DateTime.Now;

            public bool IstVeroeffentlicht { get; set; } = true;

            // BILD
            public IFormFile? BildDatei { get; set; }

            public string? AktuellesBildPfad { get; set; }

            public bool BildEntfernen { get; set; }

            public bool IstKurzmeldung { get; set; }
        }
    }
}