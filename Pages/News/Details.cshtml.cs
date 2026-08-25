using Intranet2.Datenbank.Data;
using Intranet2.Datenbank.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Intranet2.Pages.News
{
    public class DetailsModel : PageModel
    {
        private readonly DataContext _context;

        public DetailsModel(DataContext context)
        {
            _context = context;
        }

        // NEWS
        public NewsBeitrag News { get; set; } = null!;

        // SEITE LADEN
        public async Task<IActionResult> OnGetAsync(int id)
        {
            DateTime jetzt = DateTime.Now;

            NewsBeitrag? news = await _context.NewsBeitraege.AsNoTracking().FirstOrDefaultAsync(n => n.Id == id && n.IstVeroeffentlicht && n.VeroeffentlichtAm <= jetzt);

            if (news == null)
            {
                return NotFound();
            }

            News = news;

            return Page();
        }
    }
}