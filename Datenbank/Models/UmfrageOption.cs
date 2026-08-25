using System.ComponentModel.DataAnnotations;

namespace Intranet2.Datenbank.Models
{
    public class UmfrageOption
    {
        public int Id { get; set; }

        public int UmfrageId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Text { get; set; } = string.Empty;

        public int Sortierung { get; set; }

        public Umfrage Umfrage { get; set; } = null!;

        public ICollection<UmfrageStimme> Stimmen { get; set; } = new List<UmfrageStimme>();
    }
}