using Intranet2.Sicherheit;
using System.ComponentModel.DataAnnotations;

namespace Intranet2.Datenbank.Models
{
    public class Benutzer
    {
        public int Id { get; set; }

        // Eindeutiger Windows-Account, z.B. KREUTZTRAEGER\schoen
        [Required]
        [MaxLength(256)]
        public string WindowsBenutzername { get; set; } = string.Empty;

        // Anzeigename im Intranet
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        // Kann später ergänzt werden
        [MaxLength(150)]
        public string? Email { get; set; }

        // Später z.B. Benutzer oder Admin
        [Required]
        [MaxLength(30)]
        public string Rolle { get; set; } = Rollen.Benutzer;

        // Benutzer kann später gesperrt werden
        public bool IstAktiv { get; set; } = true;

        // Zeitpunkt, an dem der Benutzer erstmals ins Intranet kam
        public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

        public DateTime? LetzterMarktplatzBesuch { get; set; }
    }
}