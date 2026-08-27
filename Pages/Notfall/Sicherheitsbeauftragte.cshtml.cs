using Intranet2.Services.ActiveDirectory;
using Intranet2.Services.Fotos;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Intranet2.Pages.Notfall
{
    public class SicherheitsbeauftragtModel : PageModel
    {
        private readonly MitarbeiterService _mitarbeiterService;
        private readonly MitarbeiterFotoService _fotoService;

        public SicherheitsbeauftragtModel(MitarbeiterService mitarbeiterService,
                                           MitarbeiterFotoService fotoService)
        {
            _mitarbeiterService = mitarbeiterService;
            _fotoService = fotoService;
        }

        public List<NiederlassungSicherheit> Sicherheitsbeauftragte { get; set; } = new();

        public void OnGet()
        {
            var zuordnung = new Dictionary<string, List<string>>
            {
                { "Bergheim",   new() { "elsen" } },
                { "Bremen",     new() { "kudlorz", "nissen", "behnken" } },
                { "Flensburg",  new() { "schaeft" } },
                { "Schkeuditz", new() { "heimbold" } },
                { "Lindau",     new() { "ebert" } },
                { "Osnabrück",  new() { "rutz" } },
                { "Hamburg",    new() { "PHiller" } },
            };

            var alleMitarbeiter = _mitarbeiterService.GetMitarbeiter();

            foreach (var (niederlassung, konten) in zuordnung)
            {
                var gruppe = new NiederlassungSicherheit { Niederlassung = niederlassung };

                foreach (var konto in konten)
                {
                    var m = alleMitarbeiter.FirstOrDefault(x =>
                        x.SamAccountName.Equals(konto, StringComparison.OrdinalIgnoreCase));

                    if (m != null)
                    {
                        // ✅ BereinigeName wird jetzt tatsächlich aufgerufen
                        string vorname = BereinigeName(m.FirstName?.Trim() ?? string.Empty);
                        string nachname = BereinigeName(m.LastName?.Trim() ?? string.Empty);
                        string anzeigename = $"{vorname} {nachname}".Trim();

                        if (string.IsNullOrWhiteSpace(anzeigename))
                            anzeigename = BereinigeName(m.DisplayName);

                        gruppe.Personen.Add(new SicherheitsPerson
                        {
                            Name = anzeigename,
                            Telefon = m.TelephoneNumber,
                            Mobil = m.Mobile,
                            Email = m.Email,
                            FotoUrl = _fotoService.GetFotoUrl(nachname, vorname)
                        });
                    }
                    else
                    {
                        gruppe.Personen.Add(new SicherheitsPerson { Name = konto });
                    }
                }

                Sicherheitsbeauftragte.Add(gruppe);
            }
        }

        // ✅ Methode ist jetzt auf Klassen-Ebene – nicht mehr innerhalb der foreach-Schleife
        private static string BereinigeName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return name;

            string[] titel =
            {
                "Dipl.-Kfm.", "Dipl.-Ing.", "Dipl.-Wirt.-Ing.",
                "MBA", "M.Sc.", "B.Sc.", "M.A.", "B.A.",
                "Dr.", "Prof.", "Prof. Dr.", "Ing."
            };

            foreach (var t in titel)
                name = name.Replace(t, "", StringComparison.OrdinalIgnoreCase);

            name = name.Replace(",", "").Replace(";", "").Replace("(", "").Replace(")", "");

            return name.Trim();
        }
    }

    public class NiederlassungSicherheit
    {
        public string Niederlassung { get; set; } = string.Empty;
        public List<SicherheitsPerson> Personen { get; set; } = new();
    }

    public class SicherheitsPerson
    {
        public string Name { get; set; } = string.Empty;
        public string? Telefon { get; set; }
        public string? Mobil { get; set; }
        public string? Email { get; set; }
        public string? FotoUrl { get; set; }
    }
}
