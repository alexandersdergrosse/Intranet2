using System.ComponentModel.DataAnnotations;

namespace Intranet2.Datenbank.Models
{
    public class MarktplatzBeitrag
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string Titel { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Kategorie { get; set; } = string.Empty;

        [Required]
        [MaxLength(3000)]
        public string Beschreibung { get; set; } = string.Empty;

        public decimal? Preis { get; set; }

        [MaxLength(500)]
        public string? BildPfad { get; set; }

        public DateTime ErstelltAm { get; set; } = DateTime.UtcNow;

        // Benutzer, der den Beitrag erstellt hat
        public int BenutzerId { get; set; }

        public Benutzer Benutzer { get; set; } = null!;
    }
}