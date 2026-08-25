namespace Intranet2.Datenbank.Models
{
    public static class MarktplatzKategorien
    {
        public const string Verkaufen = "Verkaufen";

        public const string Verschenken = "Verschenken";

        public const string Gesucht = "Gesucht";

        public const string Sonstiges = "Sonstiges";

        public static readonly string[] Alle = { Verkaufen, Verschenken, Gesucht, Sonstiges };
    }
}