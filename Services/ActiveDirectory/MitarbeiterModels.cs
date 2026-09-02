namespace Intranet2.Services.ActiveDirectory
{
    public class Mitarbeiter
    {
        public string DisplayName { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string SamAccountName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;

        // Hauptabteilung aus dem AD-Feld "Abteilung"
        public string Department { get; set; } = string.Empty;

        // Unterabteilung aus dem AD-Feld "Beschreibung"
        public string Description { get; set; } = string.Empty;

        public string TelephoneNumber { get; set; } = string.Empty;
        public string Mobile { get; set; } = string.Empty;
        public string StreetAddress { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Office { get; set; } = string.Empty;
        public string EmployeeID { get; set; } = string.Empty;

        public string BereinigterVorname
        {
            get
            {
                return BereinigeName(FirstName);
            }
        }


        public string BereinigterNachname
        {
            get
            {
                return BereinigeName(LastName);
            }
        }


        public string Anzeigename
        {
            get
            {
                string vorname = BereinigterVorname;
                string nachname = BereinigterNachname;

                if (!string.IsNullOrWhiteSpace(vorname) ||
                    !string.IsNullOrWhiteSpace(nachname))
                {
                    return $"{vorname} {nachname}".Trim();
                }

                return BereinigeName(DisplayName);
            }
        }


        private static string BereinigeName(string? name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return string.Empty;
            }

            string bereinigt = name.Trim();

            string[] titel = { "Prof. Dr.", "Dipl.-Wirt.-Ing.", "Dipl.-Kfm.", "Dipl.-Ing.", "M.Sc.", "B.Sc.", "M.A.", "B.A.", "MBA", "Prof.", "Dr.", "Ing." };

            foreach (string titelEintrag in titel)
            {
                bereinigt = bereinigt.Replace(titelEintrag, "", StringComparison.OrdinalIgnoreCase);
            }

            bereinigt = bereinigt
                .Replace(",", "")
                .Replace(";", "")
                .Replace("(", "")
                .Replace(")", "");

            // Mehrfache Leerzeichen entfernen
            while (bereinigt.Contains("  "))
            {
                bereinigt = bereinigt.Replace("  ", " ");
            }

            return bereinigt.Trim();
        }


        // Niederlassung automatisch bestimmen
        public string Niederlassung
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(City))
                {
                    return City.Trim();
                }

                return string.Empty;
            }
        }

        // UNTERABTEILUNG AUS AD-BESCHREIBUNG
        public string Unterabteilung
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Description))
                {
                    return string.Empty;
                }

                string beschreibung = Description.Trim();

                int letzterTrenner = beschreibung.LastIndexOf('/');

                if (letzterTrenner >= 0)
                {
                    string letzterTeil = beschreibung[(letzterTrenner + 1)..].Trim();

                    if (string.Equals(letzterTeil, "Leitung", StringComparison.OrdinalIgnoreCase))
                    {
                        return beschreibung[..letzterTrenner].Trim();
                    }
                }
                return beschreibung;
            }
        }

        // LEITUNG ERKENNEN
        public bool IstLeitung
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Description))
                {
                    return false;
                }

                string beschreibung = Description.Trim();

                int letzterTrenner = beschreibung.LastIndexOf('/');

                if (letzterTrenner < 0)
                {
                    return false;
                }

                string letzterTeil = beschreibung[(letzterTrenner + 1)..].Trim();

                return string.Equals(letzterTeil, "Leitung", StringComparison.OrdinalIgnoreCase);
            }
        }
    }


    public class NiederlassungGruppe
    {
        public string Name { get; set; } = string.Empty;
        public List<Mitarbeiter> Mitarbeiter { get; set; } = new();
    }


    public class AbteilungGruppe
    {
        public string Name { get; set; } = string.Empty;
        public List<Mitarbeiter> Mitarbeiter { get; set; } = new();
    }


    public class UnterabteilungGruppe
    {
        public string Name { get; set; } = string.Empty;
        public List<Mitarbeiter> Mitarbeiter { get; set; } = new();
    }
}