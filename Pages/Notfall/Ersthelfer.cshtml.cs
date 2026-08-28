using Intranet2.Services.Fotos;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Intranet2.Pages.Notfall
{
    public class ErsthelferModel : PageModel
    {
        private readonly MitarbeiterFotoService _fotoService;

        public ErsthelferModel(MitarbeiterFotoService fotoService)
        {
            _fotoService = fotoService;
        }

        public List<ErsthelferEintrag> Ersthelfer { get; private set; } = new();

        public void OnGet()
        {
            var liste = new List<ErsthelferEintrag>
            {
                new() { Name = "Michael Grashoff",  Telefon = "4386736",   Mobil = "0151 42265411", Bereich = "Büro EG" },
                new() { Name = "Gunnar Förtsch",    Telefon = "4386748",   Mobil = "0151 42265448", Bereich = "Büro EG" },
                new() { Name = "Ulli Wenzel",       Telefon = "4386739",   Mobil = "0171 4029780",  Bereich = "Büro EG" },
                new() { Name = "Dennis Heppner",    Telefon = "4386720",   Mobil = "0170 7902609",  Bereich = "Büro EG" },
                new() { Name = "Andreas Fecke",     Telefon = "4386746",   Mobil = "0151 42265446", Bereich = "Büro EG" },
                new() { Name = "Kefah Alkontar",    Telefon = "22317142",  Mobil = "0170 7902603",  Bereich = "E-Abteilung" },
                new() { Name = "Stefan Gierth",     Telefon = "22317140",  Mobil = "0171 4056098",  Bereich = "E-Abteilung" },
                new() { Name = "Alexander Hohm",    Telefon = "22317133",  Mobil = "0151 42265464", Bereich = "E-Abteilung" },
                new() { Name = "Simon Sommer",      Telefon = "22317139",  Mobil = "0170 7902628",  Bereich = "E-Abteilung" },
                new() { Name = "Jessica Hotzan",    Telefon = "4386747",   Mobil = "0151 42265486", Bereich = "Lager/Werkstatt" },
            };

            foreach (var e in liste)
            {
                var teile = e.Name.Trim().Split(' ', 2);
                string vorname = teile.Length > 0 ? teile[0] : string.Empty;
                string nachname = teile.Length > 1 ? teile[1] : string.Empty;
                e.FotoUrl = _fotoService.GetFotoUrl(nachname, vorname);
            }
            Ersthelfer = liste;
        }

        public class ErsthelferEintrag
        {
            public string Name { get; set; } = string.Empty;
            public string Telefon { get; set; } = string.Empty;
            public string Mobil { get; set; } = string.Empty;
            public string Bereich { get; set; } = string.Empty;
            public string? FotoUrl { get; set; }  // ✅ NEU
        }
    }
}
