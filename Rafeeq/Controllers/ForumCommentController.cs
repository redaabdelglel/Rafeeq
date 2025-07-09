using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Rafeeq.DTOs.ForumComment;
using Rafeeq.Services.Forum.ForumComment;

namespace Rafeeq.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "MentorOrMenteePolicy")]
    //[AllowAnonymous]

    public class ForumCommentController : ControllerBase
    {
        private readonly IForumCommentService _forumCommentService;

        public ForumCommentController(IForumCommentService forumCommentService)
        {
            _forumCommentService = forumCommentService;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }
            throw new UnauthorizedAccessException("Current user ID is not available or invalid.");
        }

        // <summary>
        /// Add a new comment to a forum post.
        /// </summary>
        [HttpPost]
        [Route("~/api/forum/posts/{postId}/comments")]
        public async Task<IActionResult> AddCommentToPost(int postId, [FromBody] CreateForumCommentDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var currentUserId = GetCurrentUserId();
                var commentDto = await _forumCommentService.CreateForumCommentAsync(postId, currentUserId, dto);

                if (commentDto == null)
                {
                    return NotFound(new { success = false, message = "The post was not found or an error occurred while creating the comment." });
                }

                return StatusCode(201, new { success = true, data = commentDto });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An internal server error occurred", error = ex.Message });
            }
        }

        /// <summary>
        /// Update an existing comment.
        /// Endpoint: PUT /api/forum/comments/{commentId}
        /// </summary>
        [HttpPut("{commentId}")]
        public async Task<IActionResult> UpdateComment(int commentId, [FromBody] UpdateForumCommentDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var currentUserId = GetCurrentUserId();
                var success = await _forumCommentService.UpdateForumCommentAsync(commentId, currentUserId, dto);
                if (!success)
                {
                    return NotFound(new { success = false, message = "The comment was not found or you do not have permission to update it." });
                }
                return Ok(new { success = true, message = "Comment updated successfully." });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An internal server error occurred", error = ex.Message });
            }
        }

        [HttpDelete("{commentId}")]
        public async Task<IActionResult> DeleteComment(int commentId)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var success = await _forumCommentService.DeleteForumCommentAsync(commentId, currentUserId);
                if (!success)
                {
                    return NotFound(new { success = false, message = "The comment was not found or you do not have permission to delete it." });
                }
                return Ok(new { success = true, message = "Comment deleted successfully." });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An internal server error occurred", error = ex.Message });
            }
        }

    }
}
