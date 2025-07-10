using Rafeeq.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

public class ForumCategory
{
    [Key]
    public int CategoryId { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; }

    [StringLength(255)]
    public string? Description { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    [InverseProperty("Category")]
    public virtual ICollection<ForumPost> Posts { get; set; } = new List<ForumPost>();
}
