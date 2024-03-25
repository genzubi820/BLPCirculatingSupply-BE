using System.ComponentModel.DataAnnotations;

namespace BLPCirculatingSupply.Models
{
    public class Token
    {
        [Key]
        public int TokenId { get; set; } // For simplicity using int instead of a GUID

        [Required]
        public required string Name { get; set; }

        [Required]
        public required string TotalSupply { get; set; }

        [Required]
        public required string CirculatingSupply { get; set; }
    }
}
