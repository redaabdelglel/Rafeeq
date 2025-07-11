using Rafeeq.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class ForumPostUpvote
{
    [Key]
    public int UpvoteId { get; set; }

    [Required]
    public int PostId { get; set; }

    [Required]
    public int UserId { get; set; }

    public DateTime CreatedAt { get; set; }

    [ForeignKey(nameof(PostId))]
    public virtual ForumPost Post { get; set; }

    [ForeignKey(nameof(UserId))]
    public virtual User User { get; set; }
}
