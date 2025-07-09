using System.ComponentModel.DataAnnotations;

namespace Rafeeq.DTOs.ForumComment
{
    public class CreateForumCommentDto
    {
        [Required(ErrorMessage = "Content is required.")]
        public string Content { get; set; } = string.Empty;

        public bool IsAnswer { get; set; } = false;
    }
}
