using Intranet2.Services.ActiveDirectory;
using Intranet2.Services.Fotos;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Intranet2.Pages.Notfall
{
    public class BeauftragungsModel : PageModel
    {
        private readonly MitarbeiterService _mitarbeiterService;
        private readonly MitarbeiterFotoService _fotoService;

        public BeauftragungsModel(MitarbeiterService mitarbeiterService, MitarbeiterFotoService fotoService)
        {
            _mitarbeiterService = mitarbeiterService;
            _fotoService = fotoService;
        }

        public List<Beauftragung> Beauftragungen { get; set; } = new();

        public void OnGet()
        {
            var alle = _mitarbeiterService.GetMitarbeiter();

            BeauftragungsPerson? Person(string sam)
            {
                var m = alle.FirstOrDefault(x =>
                    x.SamAccountName.Equals(sam, StringComparison.OrdinalIgnoreCase));

                if (m == null) return null;

                // ✅ Vorname Nachname statt DisplayName
                string vorname = m.FirstName?.Trim() ?? string.Empty;
                string nachname = m.LastName?.Trim() ?? string.Empty;
                string anzeigename = $"{vorname} {nachname}".Trim();

                if (string.IsNullOrWhiteSpace(anzeigename)) anzeigename = BereinigeName(m.DisplayName);

                // Fallback auf DisplayName wenn Vor/Nachname leer
                if (string.IsNullOrWhiteSpace(anzeigename))
                    anzeigename = m.DisplayName;

                return new BeauftragungsPerson
                {
                    Name = anzeigename,
                    Telefon = m.TelephoneNumber,
                    Mobil = m.Mobile,
                    Email = m.Email,
                    FotoUrl = _fotoService.GetFotoUrl(nachname, vorname)  // Nachname für Fotosuche
                };
            }

            Beauftragungen = new List<Beauftragung>
            {
                new() {
                    Icon = "??",
                    Titel = "ASM",
                    Beschreibung = "Arbeitssicherheits-Management",
                    Personen = new() { Person("behnken") }
                },
                new() {
                    Icon = "??",
                    Titel = "Datenschutz",
                    Beschreibung = "Datenschutzbeauftragter",
                    Personen = new() { Person("behnken") }
                },
                new() {
                    Icon = "???",
                    Titel = "Sicherheitsfachkraft",
                    Beschreibung = "Bestellte Sicherheitsfachkraft",
                    Personen = new() { Person("ordemann") }
                },
                new() {
                    Icon = "??",
                    Titel = "Entsorgungsfachkraft",
                    Beschreibung = "Bestätigung der Zuverlässigkeit nach § 9 EfbV",
                    Personen = new() { Person("grashoff") }
                },
                new() {
                    Icon = "??",
                    Titel = "CE-Konformitätserklärungen",
                    Beschreibung = "Zeichnungsbefugnis",
                    Personen = new() { Person("hennings"), Person("erlhoff") }
                },
                new() {
                    Icon = "??",
                    Titel = "Betriebsarzt",
                    Beschreibung = "Arbeitsmedizinische Betreuung",
                    Personen = new() { Person("hamacher") }
                },
                new() {
                    Icon = "?",
                    Titel = "Verantwortliche Elektrofachkraft",
                    Beschreibung = "VEFK",
                    Personen = new() { Person("kopetzki") }
                },
                new() {
                    Icon = "??",
                    Titel = "DGUV3 Prüfung",
                    Beschreibung = "Prüfung elektrischer Betriebsmittel",
                    Personen = new() { Person("fecke"), Person("wening") }
                },
                new() {
                    Icon = "??",
                    Titel = "Brandschutzbeauftragter",
                    Beschreibung = "Vorbeugender Brandschutz",
                    Personen = new() { Person("behnken"), Person("maier") }
                },
                new() {
                    Icon = "?",
                    Titel = "QMB / iMS",
                    Beschreibung = "Qualitätsmanagementbeauftragter",
                    Personen = new() { Person("behnken") }
                },
                new() {
                    Icon = "??",
                    Titel = "THG-Beauftragter",
                    Beschreibung = "Treibhausgasbilanz",
                    Personen = new() { Person("eckhardt") }
                },
                new() {
                    Icon = "??",
                    Titel = "Qualitätssicherung",
                    Beschreibung = "QS-Verantwortliche",
                    Personen = new() { Person("hinz"), Person("elsen") }
                },
                new() {
                    Icon = "??",
                    Titel = "Interne Auditoren",
                    Beschreibung = "Ernannte interne Auditoren",
                    Personen = new() { Person("schaefer"), Person("schwane"), Person("heppner") }
                },
            };

            // Akademische Titel und Zusätze entfernen
            static string BereinigeName(string name)
            {
                if (string.IsNullOrWhiteSpace(name)) return name;

                string[] titel = { "Dipl.-Kfm.", "Dipl.-Ing.", "Dipl.-Wirt.-Ing.", "MBA", "M.Sc.", "B.Sc.", "M.A.", "B.A.", "Dr.", "Prof.", "Prof. Dr.", "Ing." };

                foreach (var t in titel) name = name.Replace(t, "", StringComparison.OrdinalIgnoreCase);

                return name.Trim();
            }


            // Null-Einträge (Person nicht im AD gefunden) entfernen
            foreach (var b in Beauftragungen)
                b.Personen.RemoveAll(p => p == null);
        }
    }

    public class Beauftragung
    {
        public string Icon { get; set; } = string.Empty;
        public string Titel { get; set; } = string.Empty;
        public string Beschreibung { get; set; } = string.Empty;
        public List<BeauftragungsPerson?> Personen { get; set; } = new();
    }

    public class BeauftragungsPerson
    {
        public string Name { get; set; } = string.Empty;
        public string? Telefon { get; set; }
        public string? Mobil { get; set; }
        public string? Email { get; set; }

        public string? FotoUrl { get; set; }
    }
}
