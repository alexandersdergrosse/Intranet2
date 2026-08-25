using System.Globalization;
using Intranet2.Datenbank.Data;
using Intranet2.Datenbank.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Intranet2.Pages.News
{
    public class ArchivModel : PageModel
    {
        private readonly DataContext _context;

        public ArchivModel(DataContext context)
        {
            _context = context;
        }

        public List<ArchivJahr> Jahre { get; set; } = new();

        public async Task OnGetAsync()
        {
            DateTime jetzt = DateTime.Now;


            List<NewsBeitrag> news = await _context.NewsBeitraege.AsNoTracking().Where(n => n.IstVeroeffentlicht).Where(n => n.VeroeffentlichtAm <= jetzt).OrderByDescending(n => n.VeroeffentlichtAm).ToListAsync();


            CultureInfo deutsch = CultureInfo.GetCultureInfo("de-DE");


            Jahre = news.GroupBy(n => n.VeroeffentlichtAm.Year).OrderByDescending(g => g.Key)

                    .Select(jahr => new ArchivJahr
                    {
                        Jahr = jahr.Key,

                        Monate = jahr.GroupBy(n => n.VeroeffentlichtAm.Month).OrderByDescending(m => m.Key).Select(monat =>
                                    new ArchivMonat
                                    {
                                        Monat = monat.Key,

                                        Name = deutsch.DateTimeFormat.GetMonthName(monat.Key),

                                        News = monat.OrderByDescending(n => n.VeroeffentlichtAm).ToList()
                                    }).ToList()
                    }).ToList();
        }

        public class ArchivJahr
        {
            public int Jahr { get; set; }

            public List<ArchivMonat> Monate { get; set; } = new();
        }

        public class ArchivMonat
        {
            public int Monat { get; set; }

            public string Name { get; set; } = string.Empty;

            public List<NewsBeitrag> News { get; set; } = new();
        }
    }
}