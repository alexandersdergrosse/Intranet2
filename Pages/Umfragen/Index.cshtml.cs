using Intranet2.Datenbank.Data;
using Intranet2.Datenbank.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Intranet2.Pages.Umfragen
{
    public class IndexModel : PageModel
    {
        private readonly DataContext _context;

        public IndexModel(DataContext context)
        {
            _context = context;
        }

        // UMFRAGEN
        public List<Umfrage> AktiveUmfragen { get; set; } = new();

        public List<Umfrage> VergangeneUmfragen { get; set; } = new();

        // SEITE LADEN
        public async Task OnGetAsync()
        {
            DateTime jetzt = DateTime.Now;

            List<Umfrage> umfragen = await _context.Umfragen.AsNoTracking().Include(u => u.Optionen).ThenInclude(o => o.Stimmen).Where(u => u.IstAktiv)

                    // Noch nicht gestartete Umfragen
                    // werden Mitarbeitern nicht angezeigt.
                    .Where(u => u.StartetAm <= jetzt).OrderByDescending(u => u.StartetAm).ToListAsync();

            // LAUFENDE UMFRAGEN
            AktiveUmfragen = umfragen.Where(u => !u.EndetAm.HasValue || u.EndetAm.Value >= jetzt).ToList();

            // VERGANGENE UMFRAGEN
            VergangeneUmfragen = umfragen.Where(u => u.EndetAm.HasValue && u.EndetAm.Value < jetzt).ToList();
        }
    }
}