using System.ComponentModel.DataAnnotations;

namespace Intranet2.Datenbank.Models
{
    public class BenutzerProtokoll
    {
        public int Id { get; set; }

        // Betroffener Benutzer
        public int? BenutzerId { get; set; }

        [Required]
        [MaxLength(256)]
        public string BenutzerName { get; set; } = string.Empty;

        [Required]
        [MaxLength(256)]
        public string WindowsBenutzername { get; set; } = string.Empty;

        // Was ist passiert?
        [Required]
        [MaxLength(100)]
        public string Aktion { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Feld { get; set; }

        [MaxLength(500)]
        public string? AlterWert { get; set; }

        [MaxLength(500)]
        public string? NeuerWert { get; set; }

        // Wer hat es gemacht?
        [Required]
        [MaxLength(256)]
        public string AusgefuehrtVon { get; set; } = string.Empty;

        public DateTime Zeitpunkt { get; set; } = DateTime.UtcNow;
    }
}