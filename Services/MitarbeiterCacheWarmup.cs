using Intranet2.Services.ActiveDirectory;
using Intranet2.Services.Fotos;

namespace Intranet2.Services
{
    public class MitarbeiterCacheWarmup : IHostedService
    {
        private readonly MitarbeiterService _mitarbeiterService;
        private readonly MitarbeiterFotoService _fotoService;
        private readonly ILogger<MitarbeiterCacheWarmup> _logger;

        public MitarbeiterCacheWarmup(
            MitarbeiterService mitarbeiterService,
            MitarbeiterFotoService fotoService,
            ILogger<MitarbeiterCacheWarmup> logger)
        {
            _mitarbeiterService = mitarbeiterService;
            _fotoService = fotoService;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _ = Task.Run(() =>
            {
                try
                {
                    _logger.LogInformation("Cache-Warmup: Lade Mitarbeiter aus dem Active Directory...");
                    var mitarbeiter = _mitarbeiterService.GetMitarbeiter();
                    _logger.LogInformation("Cache-Warmup: {Anzahl} Mitarbeiter geladen.", mitarbeiter.Count);

                    // Ordnerindex einmalig laden (wird intern gecacht)
                    _logger.LogInformation("Cache-Warmup: Indexiere Foto-Ordner...");
                    _fotoService.GetOrdnerIndex();

                    // ✅ Alle Fotos PARALLEL vorwärmen
                    // ✅ BereinigterNachname/BereinigterVorname – gleiche Keys wie die Seite!
                    _logger.LogInformation("Cache-Warmup: Lade Mitarbeiterfotos parallel...");
                    mitarbeiter
                        .AsParallel()
                        .WithCancellation(cancellationToken)
                        .ForAll(m => _fotoService.GetFotoUrl(
                            m.BereinigterNachname,
                            m.BereinigterVorname));

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
