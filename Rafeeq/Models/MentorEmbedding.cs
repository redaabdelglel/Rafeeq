using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rafeeq.Models
{
    public class MentorEmbedding
    {
        [Key]
        public int EmbeddingId { get; set; }

        public int? UserId { get; set; }

        public byte[]? BioEmbedding { get; set; } // Mentor bio + skills as vector

        [Column(TypeName = "datetime")]
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        [ForeignKey("UserId")]
        [InverseProperty("MentorEmbedding")]
        public virtual User? User { get; set; }
    }
}
