using Intranet2.Datenbank.Data;
using Intranet2.Datenbank.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Intranet2.Pages.News
{
    public class IndexModel : PageModel
    {
        private readonly DataContext _context;

        public IndexModel(DataContext context)
        {
            _context = context;
        }

        // FILTER
        [BindProperty(SupportsGet = true)]
        public string? Suche { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Kategorie { get; set; }

        // ERGEBNISSE
        public List<NewsBeitrag> NewsListe { get; set; } = new();

        public IReadOnlyDictionary<string, string> Kategorien
        {
            get
            {
                return NewsKategorien.Alle;
            }
        }

        // SEITE LADEN
        public async Task OnGetAsync()
        {
            DateTime jetzt = DateTime.Now;

            IQueryable<NewsBeitrag> query = _context.NewsBeitraege.AsNoTracking().Where(n => n.IstVeroeffentlicht).Where(n => !n.IstKurzmeldung).Where(n => n.VeroeffentlichtAm <= jetzt);

            // SUCHE
            if (!string.IsNullOrWhiteSpace(Suche))
            {
                string suchtext = Suche.Trim();

                query = query.Where(n => EF.Functions.Like(n.Titel, $"%{suchtext}%") || EF.Functions.Like(n.Kurztext, $"%{suchtext}%"));
            }

            // KATEGORIE
            if (!string.IsNullOrWhiteSpace(Kategorie) && NewsKategorien.IstGueltig(Kategorie))
            {
                query = query.Where(n => n.Kategorie == Kategorie);
            }

            NewsListe = await query.OrderByDescending(n => n.VeroeffentlichtAm).ToListAsync();
        }
    }
}