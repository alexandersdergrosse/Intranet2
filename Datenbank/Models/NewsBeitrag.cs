using System.ComponentModel.DataAnnotations;

namespace Intranet2.Datenbank.Models
{
    public class NewsBeitrag
    {
        public int Id { get; set; }

        // INHALT
        [Required]
        [MaxLength(200)]
        public string Titel { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string Kurztext { get; set; } = string.Empty;

        [Required]
        public string Inhalt { get; set; } = string.Empty;

        // KATEGORIE
        [Required]
        [MaxLength(100)]
        public string Kategorie { get; set; } = string.Empty;

        [Required]
        [MaxLength(30)]
        public string KategorieFarbe { get; set; } = "secondary";

        // BILD
        [MaxLength(500)]
        public string? BildPfad { get; set; }

        // VERÖFFENTLICHUNG
        public DateTime VeroeffentlichtAm { get; set; } = DateTime.UtcNow;

        public bool IstVeroeffentlicht { get; set; } = true;

        // STARTSEITE
        public bool IstKurzmeldung { get; set; } = false;

        // VERWALTUNG
        public DateTime ErstelltAm { get; set; } = DateTime.UtcNow;

        [MaxLength(256)]
        public string? ErstelltVon { get; set; }

        public DateTime? GeaendertAm { get; set; }
    }
}