using Rafeeq.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

public class ForumPost
{
    [Key]
    public int PostId { get; set; }
    [Required]
    public int UserId { get; set; }
    [Required]
    public int CategoryId { get; set; }
    [Required, StringLength(200)]
    public string Title { get; set; }
    [Required]
    public string Content { get; set; }
    public bool IsSolved { get; set; }
    public int Upvotes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }

    [ForeignKey("UserId")]
    public virtual User User { get; set; }
    [ForeignKey("CategoryId")]
    public virtual ForumCategory Category { get; set; }
    public virtual ICollection<ForumComment> Comments { get; set; }
    public virtual ICollection<ForumPostUpvote> UpvoteUsers { get; set; }
}
