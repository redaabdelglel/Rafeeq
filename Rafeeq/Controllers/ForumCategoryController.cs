using Microsoft.AspNetCore.Mvc;
using Rafeeq.DTOs.Forum;
using Rafeeq.Services.Forum;
using System.Threading.Tasks;

namespace Rafeeq.Controllers.Forum
{
    [ApiController]
    [Route("api/forum/categories")]
    public class ForumCategoryController : ControllerBase
    {
        private readonly IForumCategoryService _service;
        public ForumCategoryController(IForumCategoryService service) { _service = service; }

        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _service.GetAllCategoriesAsync();
            return Ok(categories);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCategory([FromBody] CreateForumCategoryDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _service.CreateCategoryAsync(dto);
            return CreatedAtAction(nameof(GetCategories), new { id = result.CategoryId }, result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] CreateForumCategoryDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var updated = await _service.UpdateCategoryAsync(id, dto);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var deleted = await _service.DeleteCategoryAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }
    }
}
