using System.DirectoryServices;
using Microsoft.Extensions.Caching.Memory;

namespace Intranet2.Services.ActiveDirectory
{
    public class MitarbeiterService
    {
        private const string CacheKey = "ActiveDirectory_Mitarbeiter";

        private readonly string _domain;
        private readonly string _mitarbeiterOU;
        private readonly int _cacheMinutes;

        private readonly IMemoryCache _cache;
        private readonly ILogger<MitarbeiterService> _logger;


        public MitarbeiterService(IConfiguration configuration, IMemoryCache cache, ILogger<MitarbeiterService> logger)
        {
            _domain = configuration["ActiveDirectory:Domain"] ?? throw new InvalidOperationException("ActiveDirectory:Domain wurde nicht konfiguriert.");

            _mitarbeiterOU = configuration["ActiveDirectory:MitarbeiterOU"] ?? throw new InvalidOperationException("ActiveDirectory:MitarbeiterOU wurde nicht konfiguriert.");

            _cacheMinutes = int.TryParse(configuration["ActiveDirectory:CacheMinutes"], out int cacheMinutes) ? cacheMinutes : 10;

            _cache = cache;
            _logger = logger;
        }

        // ALLE MITARBEITER AUS DEM AD LADEN
        public List<Mitarbeiter> GetMitarbeiter()
        {
            if (_cache.TryGetValue(CacheKey, out List<Mitarbeiter>? cachedMitarbeiter))
            {
                return cachedMitarbeiter ?? new List<Mitarbeiter>();
            }

            var mitarbeiter = new List<Mitarbeiter>();

            try
            {
                // LDAP-Pfad:
                //
                // LDAP://OU=Mitarbeiter,
                // DC=kreutztraeger,
                // DC=com

                string ldapPath = $"LDAP://{_mitarbeiterOU},DC={_domain.Replace(".", ",DC=")}";


                using var entry = new DirectoryEntry(ldapPath);


                using var searcher = new DirectorySearcher(entry);


                // Gesamte OU=Mitarbeiter inklusive
                // aller Unter-OUs durchsuchen.
                searcher.SearchScope = SearchScope.Subtree;


                // Gepagete Suche verwenden.
                searcher.PageSize = 1000;


                // Nur aktive Benutzer
                searcher.Filter = "(&(objectClass=user)" + "(objectCategory=person)" + "(!(userAccountControl:1.2.840.113556.1.4.803:=2)))";


                // BENÖTIGTE AD-FELDER
                searcher.PropertiesToLoad.Add("displayName");

                searcher.PropertiesToLoad.Add("givenName");

                searcher.PropertiesToLoad.Add("sn");

                searcher.PropertiesToLoad.Add("mail");

                searcher.PropertiesToLoad.Add("sAMAccountName");

                searcher.PropertiesToLoad.Add("title");

                searcher.PropertiesToLoad.Add("department");

                searcher.PropertiesToLoad.Add("telephoneNumber");

                searcher.PropertiesToLoad.Add("mobile");

                searcher.PropertiesToLoad.Add("streetAddress");

                searcher.PropertiesToLoad.Add("postalCode");

                searcher.PropertiesToLoad.Add("l");

                searcher.PropertiesToLoad.Add("physicalDeliveryOfficeName");

                searcher.PropertiesToLoad.Add("employeeID");


                using SearchResultCollection results = searcher.FindAll();


                foreach (SearchResult result in results)
                {
                    var mitarbeiterEintrag =
                        new Mitarbeiter
                        {
                            DisplayName = GetProperty(result, "displayName"),

                            FirstName =
                                GetProperty(result, "givenName"),

                            LastName = GetProperty(result, "sn"),

                            Email =
                                GetProperty(result, "mail"),

                            SamAccountName = GetProperty(result, "sAMAccountName"),

                            Title = GetProperty(result, "title"),

                            Department = GetProperty(result, "department"),

                            TelephoneNumber = GetProperty(result, "telephoneNumber"),

                            Mobile = GetProperty(result, "mobile"),

                            StreetAddress =
                                GetProperty(
                                    result,
                                    "streetAddress"),

                            PostalCode = GetProperty(result, "postalCode"),

                            City = GetProperty(result, "l"),

                            Office = GetProperty(result, "physicalDeliveryOfficeName"),

                            EmployeeID = GetProperty(result, "employeeID")
                        };


                    // Benutzer ohne Namen nicht anzeigen.
                    if (!string.IsNullOrWhiteSpace(mitarbeiterEintrag.DisplayName))
                    {
                        mitarbeiter.Add(mitarbeiterEintrag);
                    }
                }


                // SORTIEREN
                mitarbeiter = mitarbeiter
                        .OrderBy(m => m.Niederlassung)
                        .ThenBy(m => m.Title)
                        .ThenBy(m => m.DisplayName)
                        .ToList();


                // CACHE
                _cache.Set(CacheKey, mitarbeiter, TimeSpan.FromMinutes(_cacheMinutes));


                _logger.LogInformation("{Anzahl} Mitarbeiter wurden aus dem Active Directory geladen.", mitarbeiter.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Mitarbeiter konnten nicht aus dem Active Directory geladen werden.");
            }


            return mitarbeiter;
        }

        // MITARBEITER EINER NIEDERLASSUNG LADEN
        public List<Mitarbeiter> GetMitarbeiterFuerNiederlassung(string niederlassung)
        {
            return GetMitarbeiter()
                .Where(m => string.Equals(m.Niederlassung, niederlassung, StringComparison.OrdinalIgnoreCase))
                .Where(m => !IstFunktionskonto(m))
                .OrderBy(m => m.Department)
                .ThenBy(m => m.Title)
                .ThenBy(m => m.DisplayName)
                .ToList();
        }

        // FUNKTIONSKONTEN NICHT ALS MITARBEITER ANZEIGEN
        private static bool IstFunktionskonto(Mitarbeiter mitarbeiter)
        {
            if (mitarbeiter.DisplayName.Contains("Alarmhandy", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
        }


        // MITARBEITER NACH NIEDERLASSUNG GRUPPIEREN
        public List<NiederlassungGruppe>  GetMitarbeiterNachNiederlassung()
        {
            return GetMitarbeiter()
                .GroupBy(m => m.Niederlassung, StringComparer.OrdinalIgnoreCase)
                .Select(gruppe => new NiederlassungGruppe
                {
                    Name = gruppe.Key,
                    Mitarbeiter = gruppe.OrderBy(m => m.Title).ThenBy(m => m.DisplayName).ToList()
                }).OrderBy(g => g.Name).ToList();
        }

        // MITARBEITER NACH ABTEILUNG GRUPPIEREN
        public List<AbteilungGruppe> GetMitarbeiterNachAbteilung()
        {
            return GetMitarbeiter()

                // Funktionskonten nicht anzeigen
                .Where(m => !IstFunktionskonto(m))

                // Mitarbeiter ohne Abteilung auslassen
                .Where(m =>
                    !string.IsNullOrWhiteSpace(m.Department))

                // Nach Abteilung gruppieren
                .GroupBy(
                    m => m.Department.Trim(),
                    StringComparer.OrdinalIgnoreCase)

                // Gruppe erstellen
                .Select(gruppe => new AbteilungGruppe
                {
                    Name = gruppe.Key,

                    Mitarbeiter = gruppe
                        .OrderBy(m => m.Title)
                        .ThenBy(m => m.DisplayName)
                        .ToList()
                })

                // Abteilungen alphabetisch sortieren
                .OrderBy(g => g.Name)

                .ToList();
        }

        // MITARBEITER EINER ABTEILUNG LADEN
        public List<Mitarbeiter> GetMitarbeiterFuerAbteilung(
            string abteilung)
        {
            return GetMitarbeiter()

                // Funktionskonten nicht anzeigen
                .Where(m => !IstFunktionskonto(m))

                // Nur Mitarbeiter mit Abteilung
                .Where(m =>
                    !string.IsNullOrWhiteSpace(m.Department))

                // Gewählte Abteilung
                .Where(m =>
                    string.Equals(
                        m.Department.Trim(),
                        abteilung.Trim(),
                        StringComparison.OrdinalIgnoreCase))

                // Sortierung
                .OrderBy(m => m.Title)
                .ThenBy(m => m.DisplayName)

                .ToList();
        }

        // AKTUELLEN MITARBEITER ÜBER WINDOWS-BENUTZERNAMEN FINDEN
        public Mitarbeiter? GetMitarbeiterFuerBenutzername(string windowsBenutzername)
        {
            if (string.IsNullOrWhiteSpace(windowsBenutzername))
            {
                return null;
            }

            // Aus z. B. KREUZTRAEGER\schoen wird schoen
            string samAccountName = windowsBenutzername.Contains("\\") ? windowsBenutzername.Split('\\').Last() : windowsBenutzername;

            return GetMitarbeiter().FirstOrDefault(m => string.Equals(m.SamAccountName, samAccountName, StringComparison.OrdinalIgnoreCase));
        }

        // AD-EIGENSCHAFT LESEN
        private static string GetProperty(SearchResult result, string propertyName)
        {
            if (result.Properties.Contains(propertyName) && result.Properties[propertyName].Count > 0)
            {
                return result.Properties[propertyName][0] ?.ToString() ?? string.Empty;
            }

            return string.Empty;
        }
    }
}