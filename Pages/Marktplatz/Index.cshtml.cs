using Intranet2.Datenbank.Data;
using Intranet2.Datenbank.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Intranet2.Pages.Marktplatz
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly DataContext _context;

        public IndexModel(DataContext context)
        {
            _context = context;
        }

        public List<MarktplatzBeitrag> Beitraege { get; set; } = new();

        public async Task OnGetAsync()
        {
            Beitraege = await _context.MarktplatzBeitraege.AsNoTracking().Include(m => m.Benutzer).OrderByDescending(m => m.ErstelltAm).ToListAsync();

            // MARKTPLATZ ALS GELESEN MARKIEREN
            string? windowsBenutzername = User.Identity?.Name;

            if (!string.IsNullOrWhiteSpace(windowsBenutzername))
            {
                Benutzer? benutzer = await _context.Benutzer.FirstOrDefaultAsync(b => b.WindowsBenutzername == windowsBenutzername);

                if (benutzer != null)
                {
                    benutzer.LetzterMarktplatzBesuch = DateTime.Now;

                    await _context.SaveChangesAsync();

                    // Badge bereits für diesen Request ausblenden.
                    HttpContext.Items[ "NeueMarktplatzBeitraege"] = 0;
                }
            }
        }
    }
}