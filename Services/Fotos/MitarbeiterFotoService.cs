using Microsoft.Extensions.Caching.Memory;

namespace Intranet2.Services.Fotos
{
    public class MitarbeiterFotoService
    {
        private readonly string _basisPfad;
        private readonly ILogger<MitarbeiterFotoService> _logger;
        private readonly IMemoryCache _cache;

        private static readonly string[] _erlaubteEndungen =
        {
            ".jpg",
            ".jpeg",
            ".png",
            ".webp"
        };

        public MitarbeiterFotoService(IConfiguration configuration, ILogger<MitarbeiterFotoService> logger, IMemoryCache cache)
        {
            _basisPfad = configuration["Mitarbeiterfotos:Pfad"] ?? @"\\fileserver\Volume_V\mitarbeiter_fotos";

            _logger = logger;
            _cache = cache;
        }

        // FOTO-URL LADEN
        public string? GetFotoUrl( string nachname, string vorname = "")
        {
            if (string.IsNullOrWhiteSpace(nachname))
            {
                return null;
            }

            string vollname = $"{vorname} {nachname}".Trim();

            // Bereits ermitteltes Foto aus Cache verwenden
            string fotoCacheKey = $"MitarbeiterFoto_{vollname.ToLowerInvariant()}";

            if (_cache.TryGetValue(fotoCacheKey, out string? gecachteFotoUrl))
            {
                return gecachteFotoUrl;
            }

            try
            {
                // -------------------------------------------------
                // Ordnerindex laden
                //
                // Der komplette Fileserver-Ordner wird NICHT mehr
                // für jeden Mitarbeiter neu durchsucht.
                // -------------------------------------------------
                Dictionary<string, string> ordnerIndex = GetOrdnerIndex();

                if (!ordnerIndex.TryGetValue(vollname, out string? mitarbeiterOrdner))
                {
                    // Kein Fotoordner vorhanden.
                    //
                    // Auch negative Treffer kurz cachen,
                    // damit nicht immer wieder gesucht wird.

                    _cache.Set(fotoCacheKey, (string?)null, TimeSpan.FromMinutes(10));

                    return null;
                }

                // Bilder nur im konkreten Mitarbeiterordner suchen
                string[] dateien = Directory.GetFiles(mitarbeiterOrdner);

                string? erstesBild = null;
                string? exaktesBild = null;

                foreach (string datei in dateien)
                {
                    string endung = Path.GetExtension(datei);

                    if (!_erlaubteEndungen.Contains(endung, StringComparer.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    erstesBild ??= datei;

                    if (string.Equals(Path.GetFileNameWithoutExtension(datei), vollname, StringComparison.OrdinalIgnoreCase))
                    {
                        exaktesBild = datei;

                        break;
                    }
                }

                string? gefundenesBild = exaktesBild ?? erstesBild;

                if (gefundenesBild == null)
                {
                    _cache.Set(fotoCacheKey, (string?)null, TimeSpan.FromMinutes(10));

                    return null;
                }

                string fotoUrl = ZuUrl(gefundenesBild);

                // Ergebnis cachen
                _cache.Set(fotoCacheKey, fotoUrl, TimeSpan.FromMinutes(30));

                return fotoUrl;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Foto für '{Vorname} {Nachname}' konnte nicht geladen werden.", vorname, nachname);

                return null;
            }
        }

        // MITARBEITERORDNER INDEXIEREN
        private Dictionary<string, string> GetOrdnerIndex()
        {
            string cacheKey = $"MitarbeiterFotoOrdner_{_basisPfad}";

            if (_cache.TryGetValue(cacheKey, out Dictionary<string, string>? gecachterIndex) && gecachterIndex != null)
            {
                return gecachterIndex;
            }

            var ordnerIndex = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            try
            {
                if (!Directory.Exists(_basisPfad))
                {
                    _logger.LogWarning("Der Mitarbeiterfoto-Pfad '{Pfad}' ist nicht erreichbar.", _basisPfad);

                    return ordnerIndex;
                }

                // Nur EINMAL alle Mitarbeiterordner lesen
                foreach (string ordner in Directory.EnumerateDirectories(_basisPfad))
                {
                    string? ordnerName = Path.GetFileName(ordner);

                    if (string.IsNullOrWhiteSpace(ordnerName))
                    {
                        continue;
                    }

                    ordnerIndex[ordnerName.Trim()] = ordner;
                }

                // Ordnerliste 30 Minuten zwischenspeichern
                _cache.Set(cacheKey, ordnerIndex, TimeSpan.FromMinutes(30));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Die Mitarbeiterfoto-Ordner konnten nicht geladen werden.");
            }

            return ordnerIndex;
        }

        // DATEIPFAD -> URL
        private string ZuUrl(string dateiPfad)
        {
            string relativ = Path.GetRelativePath(_basisPfad, dateiPfad).Replace('\\', '/');

            relativ = string.Join("/", relativ.Split('/').Select(Uri.EscapeDataString));

            return $"/mitarbeiterfotos/{relativ}";
        }
    }
}