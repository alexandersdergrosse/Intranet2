using System.Net;
using HtmlAgilityPack;
using Microsoft.Extensions.Caching.Memory;

namespace Intranet2.Services.Jobs
{
    /// <summary>
    /// Lädt die veröffentlichten Stellenangebote direkt von der
    /// offiziellen Kreutzträger-Stellenangebotsseite.
    /// </summary>
    public class StellenService
    {
        private const string CacheKey = "Kreutztraeger_Stellenangebote";

        private const string StellenangeboteAdresse = "karriere/stellenangebote/";

        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private readonly ILogger<StellenService> _logger;

        public StellenService(HttpClient httpClient, IMemoryCache cache, ILogger<StellenService> logger)
        {
            _httpClient = httpClient;
            _cache = cache;
            _logger = logger;
        }

        /// <summary>
        /// Gibt alle aktuell auf der öffentlichen Webseite
        /// veröffentlichten Stellenangebote zurück.
        /// </summary>
        public async Task<List<Stellenangebot>> GetStellenangeboteAsync()
        {
            if (_cache.TryGetValue(CacheKey, out List<Stellenangebot>? cachedJobs))
            {
                return cachedJobs ?? new List<Stellenangebot>();
            }

            try
            {
                List<Stellenangebot> stellenangebote = await LadeStellenangeboteAsync();

                // Zur Sicherheit doppelte Links entfernen.
                stellenangebote = stellenangebote.GroupBy(s => s.Url, StringComparer.OrdinalIgnoreCase).Select(g => g.First()).ToList();

                // Die Webseite muss nicht bei jedem
                // Intranet-Aufruf erneut geladen werden.
                if (stellenangebote.Count > 0)
                {
                    _cache.Set(CacheKey, stellenangebote, TimeSpan.FromMinutes(15));
                }

                _logger.LogInformation("{Anzahl} Stellenangebote wurden von {Adresse} geladen.", stellenangebote.Count, StellenangeboteAdresse);

                return stellenangebote;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Die Stellenangebote konnten nicht von der Kreutzträger-Webseite geladen werden.");

                return new List<Stellenangebot>();
            }
        }

        /// <summary>
        /// Lädt die HTML-Seite mit den Stellenangeboten.
        /// </summary>
        private async Task<List<Stellenangebot>> LadeStellenangeboteAsync()
        {
            using HttpResponseMessage response = await _httpClient.GetAsync(StellenangeboteAdresse);

            response.EnsureSuccessStatusCode();

            string html = await response.Content.ReadAsStringAsync();

            return LeseStellenAusHtml(html);
        }

        /// <summary>
        /// Liest ausschließlich den Bereich
        /// "Aktuelle Stellenangebote" aus der Webseite.
        /// </summary>
        private List<Stellenangebot> LeseStellenAusHtml(string html)
        {
            var document = new HtmlDocument();

            document.LoadHtml(html);

            var stellenangebote = new List<Stellenangebot>();


            bool imStellenbereich = false;

            foreach (HtmlNode node in document.DocumentNode.Descendants())
            {
                // START DES STELLENBEREICHS FINDEN
                if (!imStellenbereich && IstUeberschrift(node))
                {
                    string ueberschrift = Bereinigen(node.InnerText);

                    if (ueberschrift.Equals("Aktuelle Stellenangebote", StringComparison.OrdinalIgnoreCase))
                    {
                        imStellenbereich = true;
                    }
                    continue;
                }

                if (!imStellenbereich)
                {
                    continue;
                }

                // Die einzelnen Stellen sind auf der Webseite
                // als h5-Überschriften mit Link aufgebaut.
                if (!node.Name.Equals("h5", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                HtmlNode? link = node.SelectSingleNode(".//a[@href]");

                if (link == null)
                {
                    continue;
                }

                string titel = Bereinigen(link.InnerText);

                string href = link.GetAttributeValue("href", string.Empty);

                if (string.IsNullOrWhiteSpace(titel) || string.IsNullOrWhiteSpace(href))
                {
                    continue;
                }

                // Relative und absolute Links unterstützen.
                if (!Uri.TryCreate(_httpClient.BaseAddress, href, out Uri? absoluteUrl))
                {
                    continue;
                }

                // Nur Stellenlinks der Kreutzträger-Webseite.
                if (!string.Equals(absoluteUrl.Host, "kreutztraeger-kaeltetechnik.de", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }


                stellenangebote.Add(new Stellenangebot
                {
                        Titel = titel,

                        Beschreibung = LeseBeschreibung(node),

                        Url = absoluteUrl.ToString() 
                });
            }

            return stellenangebote;
        }

        /// <summary>
        /// Liest den kurzen Beschreibungstext bzw.
        /// Einsatzort oberhalb eines Stellenangebotes aus.
        /// </summary>
        private static string LeseBeschreibung(HtmlNode stellenUeberschrift)
        {
            // Zuerst innerhalb des zugehörigen
            // Stellencontainers suchen.
            HtmlNode? container = stellenUeberschrift.Ancestors().FirstOrDefault(node => HatCssKlasse(node, "post-listing-container"));

            HtmlNode? beschreibung = container?.SelectSingleNode(".//p");

            if (beschreibung != null)
            {
                return Bereinigen(beschreibung.InnerText);
            }

            // Fallback für die aktuelle Stellenangebotsseite:
            // letzten Absatz vor der Stellenüberschrift verwenden.
            beschreibung = stellenUeberschrift.SelectSingleNode("preceding::p[1]");

            return beschreibung == null ? string.Empty : Bereinigen(beschreibung.InnerText);
        }

        /// <summary>
        /// Prüft, ob ein HTML-Element eine bestimmte
        /// CSS-Klasse besitzt.
        /// </summary>
        private static bool HatCssKlasse(HtmlNode node, string klasse)
        {
            string classValue = node.GetAttributeValue("class", string.Empty);

            return classValue.Split(' ', StringSplitOptions.RemoveEmptyEntries).Contains(klasse, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Prüft, ob der HTML-Knoten eine Überschrift ist.
        /// </summary>
        private static bool IstUeberschrift(HtmlNode node)
        {
            return node.Name is "h1" or "h2" or "h3" or "h4" or "h5" or "h6";
        }

        /// <summary>
        /// Entfernt HTML-Codierung und überflüssige
        /// Leerzeichen aus eingelesenen Texten.
        /// </summary>
        private static string Bereinigen(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            string decoded = WebUtility.HtmlDecode(text);

            return string.Join(" ", decoded.Split(new[] { ' ', '\r', '\n', '\t' },StringSplitOptions.RemoveEmptyEntries));
        }
    }
}