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
    public class DetailsModel : PageModel
    {
        private readonly DataContext _context;
        private readonly IWebHostEnvironment _environment;

        public DetailsModel(DataContext context, IWebHostEnvironment environment)
        {
            _context = context; 
            _environment = environment;
        }

        public MarktplatzBeitrag Beitrag { get; set; } = null!;

        public bool DarfLoeschen { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            MarktplatzBeitrag? beitrag = await _context.MarktplatzBeitraege.AsNoTracking().Include(m => m.Benutzer).FirstOrDefaultAsync(m => m.Id == id);

            if (beitrag == null)
            {
                return NotFound();
            }

            Beitrag = beitrag;

            DarfLoeschen = User.IsInRole(Rollen.Admin) || string.Equals(beitrag.Benutzer.WindowsBenutzername, User.Identity?.Name, StringComparison.OrdinalIgnoreCase);

            return Page();
        }

        public async Task<IActionResult> OnPostLoeschenAsync(int id)
        {
            MarktplatzBeitrag? beitrag = await _context.MarktplatzBeitraege.Include(m => m.Benutzer).FirstOrDefaultAsync(m => m.Id == id);

            if (beitrag == null)
            {
                return NotFound();
            }

            bool darfLoeschen = User.IsInRole(Rollen.Admin) || string.Equals(beitrag.Benutzer.WindowsBenutzername, User.Identity?.Name, StringComparison.OrdinalIgnoreCase);

            if (!darfLoeschen)
            {
                return Forbid();
            }

            // Bild löschen
            if (!string.IsNullOrWhiteSpace(beitrag.BildPfad))
            {
                string relativerPfad = beitrag.BildPfad.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);

                string kompletterPfad = Path.Combine(_environment.WebRootPath, relativerPfad);

                if (System.IO.File.Exists(kompletterPfad))
                {
                    System.IO.File.Delete(kompletterPfad);
                }
            }

            _context.MarktplatzBeitraege.Remove(beitrag);

            await _context.SaveChangesAsync();

            return RedirectToPage("/Marktplatz/Index");
        }
    }
}