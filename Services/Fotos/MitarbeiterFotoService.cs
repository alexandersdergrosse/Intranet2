namespace Intranet2.Services.Fotos
{
    public class MitarbeiterFotoService
    {
        private readonly string _basisPfad;
        private readonly ILogger<MitarbeiterFotoService> _logger;
        private static readonly string[] _erlaubteEndungen = { ".jpg", ".jpeg", ".png", ".webp" };

        public MitarbeiterFotoService(IConfiguration configuration, ILogger<MitarbeiterFotoService> logger)
        {
            _basisPfad = configuration["Mitarbeiterfotos:Pfad"] ?? @"\\fileserver\Volume_V\mitarbeiter_fotos";
            _logger = logger;
        }

        public string? GetFotoUrl(string nachname, string vorname = "")
        {
            if (string.IsNullOrWhiteSpace(nachname)) return null;

            try
            {
                string vollname = $"{vorname} {nachname}".Trim();

                string? mitarbeiterOrdner = Directory.GetDirectories(_basisPfad).FirstOrDefault(d => Path.GetFileName(d).Equals(vollname, StringComparison.OrdinalIgnoreCase));

                if (mitarbeiterOrdner == null) return null;

                var bilder = Directory.GetFiles(mitarbeiterOrdner).Where(f => _erlaubteEndungen.Contains(Path.GetExtension(f).ToLowerInvariant())).ToList();

                //Nur ein Durchlauf:
                string? erstesBild = bilder.FirstOrDefault();
                if (erstesBild == null) return null;

                // Exaktes Bild bevorzugen:
                string? exaktesBild = bilder.FirstOrDefault(f => Path.GetFileNameWithoutExtension(f).Equals(vollname, StringComparison.OrdinalIgnoreCase));

                //Exaktes Bild oder Fallback — kein doppelter Durchlauf:
                return ZuUrl(exaktesBild ?? erstesBild);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Foto für '{Vorname} {Nachname}' konnte nicht geladen werden.", vorname, nachname);
                return null;
            }
        }


        private string ZuUrl(string dateiPfad)
        {
            string relativ = Path.GetRelativePath(_basisPfad, dateiPfad)
                                 .Replace('\\', '/');
            // Leerzeichen und Sonderzeichen URL-encoden
            relativ = string.Join("/",
                relativ.Split('/').Select(Uri.EscapeDataString));
            return $"/mitarbeiterfotos/{relativ}";
        }
    }
}
