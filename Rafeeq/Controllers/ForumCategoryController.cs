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
    }
}
