namespace Intranet2.Datenbank.Models
{
    public static class NewsKategorien
    {
        public static readonly IReadOnlyDictionary<string, string> Alle =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "Allgemein", "secondary" },
                { "Unternehmen", "primary" },
                { "Personal", "info" },
                { "Events", "success" },
                { "Weiterbildung", "warning" },
                { "IT", "dark" },
                { "Sicherheit", "danger" }
            };


        public static bool IstGueltig(string? kategorie)
        {
            if (string.IsNullOrWhiteSpace(kategorie))
            {
                return false;
            }

            return Alle.ContainsKey(kategorie.Trim());
        }


        public static string FarbeFuer(string? kategorie)
        {
            if (string.IsNullOrWhiteSpace(kategorie))
            {
                return "secondary";
            }


            if (Alle.TryGetValue(kategorie.Trim(), out string? farbe))
            {
                return farbe;
            }


            return "secondary";
        }
    }
}