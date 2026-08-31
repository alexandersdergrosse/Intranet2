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


        // Unterabteilung aus Beschreibung
        public string Unterabteilung
        {
            get
            {
                return Description?.Trim() ?? string.Empty;
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