using System.ComponentModel.DataAnnotations;

namespace Intranet2.Datenbank.Models
{
    public class UmfrageStimme
    {
        public int Id { get; set; }

        public int UmfrageId { get; set; }

        public int UmfrageOptionId { get; set; }

        [Required]
        [MaxLength(256)]
        public string WindowsBenutzername { get; set; } = string.Empty;

        public DateTime AbgestimmtAm { get; set; } = DateTime.UtcNow;

        public Umfrage Umfrage { get; set; } = null!;

        public UmfrageOption UmfrageOption { get; set; } = null!;
    }
}