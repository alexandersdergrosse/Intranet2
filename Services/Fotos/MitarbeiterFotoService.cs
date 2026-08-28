namespace Intranet2.Services.Fotos
{
    public class MitarbeiterFotoService
    {
        private readonly string _basisPfad;
        private readonly ILogger<MitarbeiterFotoService> _logger;
        private static readonly string[] _erlaubteEndungen = { ".jpg", ".jpeg", ".png", ".webp" };

        public MitarbeiterFotoService(IConfiguration configuration, ILogger<MitarbeiterFotoService> logger)
        {
            _basisPfad = configuration["Mitarbeiterfotos:Pfad"]
                         ?? @"\\fileserver\Volume_V\mitarbeiter_fotos";
            _logger = logger;
        }

        public string? GetFotoUrl(string nachname, string vorname = "")
        {
            if (string.IsNullOrWhiteSpace(nachname)) return null;

            try
            {
                // Ordnername = "Vorname Nachname"
                string vollname = $"{vorname} {nachname}".Trim();

                _logger.LogInformation("Suche Foto für: '{Vollname}'", vollname);

                // Ordner suchen (case-insensitive)
                string? mitarbeiterOrdner = Directory
                    .GetDirectories(_basisPfad)
                    .FirstOrDefault(d => Path.GetFileName(d)
                        .Equals(vollname, StringComparison.OrdinalIgnoreCase));

                if (mitarbeiterOrdner == null)
                {
                    _logger.LogInformation("Kein Ordner gefunden für: '{Vollname}'", vollname);
                    return null;
                }

                _logger.LogInformation("Ordner gefunden: {Ordner}", mitarbeiterOrdner);

                // Bild suchen – zuerst exakter Name, dann erstes Bild im Ordner
                var bilder = Directory
                    .GetFiles(mitarbeiterOrdner)
                    .Where(f => _erlaubteEndungen.Contains(
                        Path.GetExtension(f).ToLowerInvariant()))
                    .ToList();

                if (!bilder.Any())
                {
                    _logger.LogInformation("Keine Bilder im Ordner gefunden.");
                    return null;
                }

                // 1. Bild mit exakt dem Vollnamen suchen (z.B. "Jens Behnken.jpg")
                string? exaktesBild = bilder.FirstOrDefault(f =>
                    Path.GetFileNameWithoutExtension(f)
                        .Equals(vollname, StringComparison.OrdinalIgnoreCase));

                if (exaktesBild != null)
                {
                    _logger.LogInformation("✅ Exaktes Bild gefunden: {Datei}", Path.GetFileName(exaktesBild));
                    return ZuUrl(exaktesBild);
                }

                // 2. Erstes Bild im Ordner als Fallback
                _logger.LogInformation("✅ Erstes Bild als Fallback: {Datei}", Path.GetFileName(bilder.First()));
                return ZuUrl(bilder.First());
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Foto für '{Vorname} {Nachname}' konnte nicht geladen werden.",
                    vorname, nachname);
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
