using System.ComponentModel.DataAnnotations;

namespace Rafeeq.DTOs.ForumComment
{
    public class UpdateForumCommentDto
    {
        [Required(ErrorMessage = "Content is required.")]
        public string Content { get; set; } = string.Empty;
    }
}
