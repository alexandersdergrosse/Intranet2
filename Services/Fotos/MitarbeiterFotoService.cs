using Microsoft.Extensions.Caching.Memory;

namespace Intranet2.Services.Fotos
{
    public class MitarbeiterFotoService
    {
        private readonly string _basisPfad;
        private readonly ILogger<MitarbeiterFotoService> _logger;
        private readonly IMemoryCache _cache;

        // Foto-Cache: 8 Stunden – läuft nicht mehr während der Arbeitszeit ab
        private static readonly TimeSpan FotoCacheDauer = TimeSpan.FromHours(8);
        // Ordnerindex: ebenfalls 8 Stunden
        private static readonly TimeSpan OrdnerIndexCacheDauer = TimeSpan.FromHours(8);
        // Negative Treffer (kein Foto vorhanden): 2 Stunden
        private static readonly TimeSpan NegativCacheDauer = TimeSpan.FromHours(2);

        private static readonly string[] _erlaubteEndungen =
        {
            ".jpg", ".jpeg", ".png", ".webp"
        };

        public MitarbeiterFotoService(IConfiguration configuration, ILogger<MitarbeiterFotoService> logger, IMemoryCache cache)
        {
            _basisPfad = configuration["Mitarbeiterfotos:Pfad"] ?? @"\\192.168.165.13\Volume_V\mitarbeiter_fotos";
            _logger = logger;
            _cache = cache;
        }

        // FOTO-URL LADEN
        public string? GetFotoUrl(string nachname, string vorname = "")
        {
            if (string.IsNullOrWhiteSpace(nachname))
                return null;

            string vollname = $"{vorname} {nachname}".Trim();
            string fotoCacheKey = $"MitarbeiterFoto_{vollname.ToLowerInvariant()}";

            if (_cache.TryGetValue(fotoCacheKey, out string? gecachteFotoUrl))
                return gecachteFotoUrl;

            try
            {
                Dictionary<string, string> ordnerIndex = GetOrdnerIndex();

                if (!ordnerIndex.TryGetValue(vollname, out string? mitarbeiterOrdner))
                {
                    _cache.Set(fotoCacheKey, (string?)null, NegativCacheDauer);
                    return null;
                }

                string[] dateien = Directory.GetFiles(mitarbeiterOrdner);
                string? erstesBild = null;
                string? exaktesBild = null;

                foreach (string datei in dateien)
                {
                    string endung = Path.GetExtension(datei);
                    if (!_erlaubteEndungen.Contains(endung, StringComparer.OrdinalIgnoreCase))
                        continue;

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
                    _cache.Set(fotoCacheKey, (string?)null, NegativCacheDauer);
                    return null;
                }

                string fotoUrl = ZuUrl(gefundenesBild);
                _cache.Set(fotoCacheKey, fotoUrl, FotoCacheDauer);
                return fotoUrl;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Foto für '{Vorname} {Nachname}' konnte nicht geladen werden.", vorname, nachname);
                return null;
            }
        }

        // MITARBEITERORDNER INDEXIEREN
        public Dictionary<string, string> GetOrdnerIndex()
        {
            string cacheKey = $"MitarbeiterFotoOrdner_{_basisPfad}";

            if (_cache.TryGetValue(cacheKey, out Dictionary<string, string>? gecachterIndex) && gecachterIndex != null)
                return gecachterIndex;

            var ordnerIndex = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            try
            {
                if (!Directory.Exists(_basisPfad))
                {
                    _logger.LogWarning("Der Mitarbeiterfoto-Pfad '{Pfad}' ist nicht erreichbar.", _basisPfad);
                    return ordnerIndex;
                }

                foreach (string ordner in Directory.EnumerateDirectories(_basisPfad))
                {
                    string? ordnerName = Path.GetFileName(ordner);
                    if (!string.IsNullOrWhiteSpace(ordnerName)) ordnerIndex[ordnerName.Trim()] = ordner;
                }

                _cache.Set(cacheKey, ordnerIndex, OrdnerIndexCacheDauer);
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
