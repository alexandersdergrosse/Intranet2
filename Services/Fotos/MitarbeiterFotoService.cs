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
                _logger.LogInformation("=== FOTO SUCHE: Vorname='{Vorname}' Nachname='{Nachname}'", vorname, nachname);
                _logger.LogInformation("Basispfad: {Pfad}", _basisPfad);
                _logger.LogInformation("Basispfad existiert: {Exists}", Directory.Exists(_basisPfad));

                // Ordnersuche
                string? mitarbeiterOrdner = null;

                if (!string.IsNullOrWhiteSpace(vorname))
                {
                    char initial = vorname[0];
                    var variantenMitInitial = new[]
                    {
                $"{nachname}{initial}",
                $"{nachname} {initial}",
                $"{nachname}_{initial}",
            };

                    _logger.LogInformation("Suche Ordner mit Varianten: {Varianten}",
                        string.Join(", ", variantenMitInitial));

                    mitarbeiterOrdner = Directory
                        .GetDirectories(_basisPfad)
                        .FirstOrDefault(d =>
                        {
                            string ordnerName = Path.GetFileName(d);
                            return variantenMitInitial.Any(v =>
                                ordnerName.Equals(v, StringComparison.OrdinalIgnoreCase));
                        });
                }

                if (mitarbeiterOrdner == null)
                {
                    _logger.LogInformation("Kein Initial-Ordner gefunden, suche '{Nachname}'", nachname);
                    mitarbeiterOrdner = Directory
                        .GetDirectories(_basisPfad)
                        .FirstOrDefault(d => Path.GetFileName(d)
                            .Equals(nachname.Trim(), StringComparison.OrdinalIgnoreCase));
                }

                _logger.LogInformation("Gefundener Ordner: {Ordner}", mitarbeiterOrdner ?? "KEINER");

                if (mitarbeiterOrdner == null) return null;

                // Jahresordner
                var jahresOrdner = Directory
                    .GetDirectories(mitarbeiterOrdner)
                    .Where(d => int.TryParse(Path.GetFileName(d), out _))
                    .OrderByDescending(d => Path.GetFileName(d))
                    .ToList();

                _logger.LogInformation("Jahresordner gefunden: {Anzahl} → {Ordner}",
                    jahresOrdner.Count, string.Join(", ", jahresOrdner.Select(Path.GetFileName)));

                if (jahresOrdner.Any())
                {
                    foreach (var jahr in jahresOrdner)
                    {
                        var alleBilder = Directory.GetFiles(jahr).Where(IstBild).ToList();
                        _logger.LogInformation("Bilder in {Jahr}: {Bilder}",
                            Path.GetFileName(jahr),
                            string.Join(", ", alleBilder.Select(Path.GetFileName)));

                        var bilder = alleBilder
                            .Where(f => BildPasstZuPerson(f, vorname, nachname))
                            .ToList();

                        _logger.LogInformation("Bilder nach Filter: {Bilder}",
                            string.Join(", ", bilder.Select(Path.GetFileName)));

                        if (!bilder.Any()) continue;

                        string? nurTelefon = bilder.FirstOrDefault(f => DateiEnthaelt(f, "Telefon"));
                        if (nurTelefon != null)
                        {
                            _logger.LogInformation("✅ Telefon-Bild gefunden: {Datei}", Path.GetFileName(nurTelefon));
                            return ZuUrl(nurTelefon);
                        }

                        _logger.LogInformation("✅ Erstes Bild: {Datei}", Path.GetFileName(bilder.First()));
                        return ZuUrl(bilder.First());
                    }
                }

                // Direkte Bilder
                var direkteBilder = Directory.GetFiles(mitarbeiterOrdner)
                    .Where(IstBild)
                    .Where(f => BildPasstZuPerson(f, vorname, nachname))
                    .ToList();

                _logger.LogInformation("Direkte Bilder nach Filter: {Bilder}",
                    string.Join(", ", direkteBilder.Select(Path.GetFileName)));

                if (direkteBilder.Any())
                    return ZuUrl(direkteBilder.First());
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Foto für {Vorname} {Nachname} konnte nicht geladen werden.", vorname, nachname);
            }

            return null;
        }

        private bool BildPasstZuPerson(string pfad, string vorname, string nachname)
        {
            if (string.IsNullOrWhiteSpace(vorname)) return true;

            string dateiname = Path.GetFileNameWithoutExtension(pfad);

            // ✅ Gesuchter Vorname im Dateinamen → passt
            if (dateiname.Contains(vorname, StringComparison.OrdinalIgnoreCase))
                return true;

            // ✅ Bindestrich-Vorname: ersten Teil prüfen ("Jan-Peter" → "Jan")
            if (vorname.Contains('-'))
            {
                string ersterVorname = vorname.Split('-')[0];
                if (dateiname.Contains(ersterVorname, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            // Bekannte Begriffe entfernen
            string bereinigt = dateiname
                .Replace(nachname, "", StringComparison.OrdinalIgnoreCase)
                .Replace("Telefon", "", StringComparison.OrdinalIgnoreCase)
                .Replace("Foto", "", StringComparison.OrdinalIgnoreCase)
                .Trim('_', '-', ' ');

            if (string.IsNullOrWhiteSpace(bereinigt)) return true;
            if (int.TryParse(bereinigt.Trim('_', '-', ' '), out _)) return true;

            // ✅ NEU: Übrig gebliebenen Rest in Teile zerlegen
            // und prüfen ob alle Teile zum Vornamen passen
            // "Jan-Peter" → ["Jan", "Peter"] → beide sind Teile des gesuchten Vornamens
            var vornameTeile = vorname
                .Split(new[] { '-', ' ' }, StringSplitOptions.RemoveEmptyEntries);

            var bereinigtTeile = bereinigt
                .Split(new[] { '_', '-', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Where(t => !int.TryParse(t, out _))
                .ToList();

            // Alle übrigen Teile müssen zum Vornamen gehören
            bool allePassend = bereinigtTeile.All(teil =>
                vornameTeile.Any(vt =>
                    vt.Equals(teil, StringComparison.OrdinalIgnoreCase)));

            if (allePassend) return true;

            // Anderer Vorname erkannt → ablehnen
            return false;
        }


        private bool DateiEnthaelt(string pfad, string begriff)
        {
            if (string.IsNullOrWhiteSpace(begriff)) return false;

            string dateiname = Path.GetFileNameWithoutExtension(pfad);

            // Direkte Suche
            if (dateiname.Contains(begriff, StringComparison.OrdinalIgnoreCase))
                return true;

            // ✅ NEU: Bei Bindestrich auch ersten Teil suchen
            // "Jan-Peter" → auch nach "Jan" suchen
            if (begriff.Contains('-'))
            {
                string ersterTeil = begriff.Split('-')[0];
                return dateiname.Contains(ersterTeil, StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }


        private bool IstBild(string pfad) =>
            _erlaubteEndungen.Contains(
                Path.GetExtension(pfad).ToLowerInvariant());

        private string ZuUrl(string dateiPfad)
        {
            string relativ = Path.GetRelativePath(_basisPfad, dateiPfad)
                                 .Replace('\\', '/');
            return $"/mitarbeiterfotos/{relativ}";
        }
    }
}
