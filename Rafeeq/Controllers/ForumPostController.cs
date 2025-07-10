using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rafeeq.DTOs.Forum;
using Rafeeq.Services.Forum;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Rafeeq.Controllers.Forum
{
    [ApiController]
    [Route("api/forum/posts")]
    public class ForumPostController : ControllerBase
    {
        private readonly IForumPostMgmtService _service;
        public ForumPostController(IForumPostMgmtService service) { _service = service; }

        [HttpGet]
        public async Task<IActionResult> GetPosts(
            [FromQuery] int? categoryId = null,
            [FromQuery] string? search = null,
            [FromQuery] string? sortBy = "recent",
            [FromQuery] bool? isSolved = null)
        {
            var posts = await _service.GetAllAsync(categoryId, search, sortBy, isSolved);
            return Ok(posts);
        }

        [HttpGet("{postId}")]
        public async Task<IActionResult> GetPost(int postId)
        {
            var post = await _service.GetByIdAsync(postId);
            if (post == null) return NotFound();
            return Ok(post);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreatePost([FromBody] CreateForumPostDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var post = await _service.CreateAsync(dto, userId);
            return CreatedAtAction(nameof(GetPost), new { postId = post.PostId }, post);
        }

        [HttpPut("{postId}")]
        [Authorize]
        public async Task<IActionResult> UpdatePost(int postId, [FromBody] UpdateForumPostDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var success = await _service.UpdateAsync(postId, dto, userId);
            if (!success) return Forbid();
            return NoContent();
        }

        [HttpDelete("{postId}")]
        [Authorize]
        public async Task<IActionResult> DeletePost(int postId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var success = await _service.DeleteAsync(postId, userId);
            if (!success) return Forbid();
            return NoContent();
        }

        [HttpPost("{postId}/upvote")]
        [Authorize]
        public async Task<IActionResult> Upvote(int postId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var success = await _service.UpvoteAsync(postId, userId);
            if (!success) return BadRequest("Already upvoted or post not found.");
            return Ok();
        }

        [HttpDelete("{postId}/upvote")]
        [Authorize]
        public async Task<IActionResult> RemoveUpvote(int postId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var success = await _service.RemoveUpvoteAsync(postId, userId);
            if (!success) return BadRequest("Not upvoted or post not found.");
            return Ok();
        }

        [HttpPost("{postId}/solve")]
        [Authorize]
        public async Task<IActionResult> MarkAsSolved(int postId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var success = await _service.MarkAsSolvedAsync(postId, userId);
            if (!success) return Forbid();
            return Ok();
        }

        [HttpGet("/api/forum/users/{userId}/posts")]
        public async Task<IActionResult> GetUserPosts(int userId)
        {
            var posts = await _service.GetByUserIdAsync(userId);
            return Ok(posts);
        }

        // User reports a post
        [HttpPost("{postId}/report")]
        [Authorize]
        public async Task<IActionResult> ReportPost(int postId, [FromBody] CreateForumPostReportDto dto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var success = await _service.ReportPostAsync(postId, userId, dto.Reason);
            if (!success) return BadRequest("Already reported or post not found.");
            return Ok();
        }

        // Admin: get all reports
        [HttpGet("/api/admin/forum/reports")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllReports()
        {
            var reports = await _service.GetAllReportsAsync();
            return Ok(reports);
        }


        // Admin: take action on a report
        [HttpPut("/api/admin/forum/reports/{reportId}/action")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> TakeActionOnReport(int reportId, [FromBody] AdminReportActionDto dto)
        {
            var success = await _service.TakeActionOnReportAsync(reportId, dto.Action, dto.AdminNote);
            if (!success) return BadRequest("Invalid action or report not found.");
            return Ok();
        }

        [HttpPost("{postId}/pin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PinPost(int postId)
        {
            var success = await _service.PinPostAsync(postId);
            if (!success) return NotFound();
            return Ok();
        }

        [HttpPost("{postId}/unpin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UnpinPost(int postId)
        {
            var success = await _service.UnpinPostAsync(postId);
            if (!success) return NotFound();
            return Ok();
        }


    }
}
