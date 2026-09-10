using Intranet2.Services.ActiveDirectory;
using Intranet2.Services.Fotos;

namespace Intranet2.Services
{
    /// <summary>
    /// Wärmt den Mitarbeiter- und Foto-Cache beim App-Start vor,
    /// damit die Ansprechpartner-Seiten sofort schnell laden.
    /// </summary>
    public class MitarbeiterCacheWarmup : IHostedService
    {
        private readonly MitarbeiterService _mitarbeiterService;
        private readonly MitarbeiterFotoService _fotoService;
        private readonly ILogger<MitarbeiterCacheWarmup> _logger;

        public MitarbeiterCacheWarmup(MitarbeiterService mitarbeiterService, MitarbeiterFotoService fotoService, ILogger<MitarbeiterCacheWarmup> logger)
        {
            _mitarbeiterService = mitarbeiterService;
            _fotoService = fotoService;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            // Im Hintergrund starten – App startet sofort,
            // Cache wird parallel befüllt
            _ = Task.Run(() =>
            {
                try
                {
                    _logger.LogInformation("Cache-Warmup: Lade Mitarbeiter aus dem Active Directory...");
                    var mitarbeiter = _mitarbeiterService.GetMitarbeiter();
                    _logger.LogInformation("Cache-Warmup: {Anzahl} Mitarbeiter geladen.", mitarbeiter.Count);

                    // Foto-Ordnerindex ebenfalls vorwärmen
                    _logger.LogInformation("Cache-Warmup: Lade Mitarbeiterfotos...");
                    foreach (var m in mitarbeiter)
                    {
                        if (cancellationToken.IsCancellationRequested) break;
                        _fotoService.GetFotoUrl(m.LastName, m.FirstName);
                    }
                    _logger.LogInformation("Cache-Warmup: Abgeschlossen.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Cache-Warmup fehlgeschlagen.");
                }
            }, cancellationToken);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
