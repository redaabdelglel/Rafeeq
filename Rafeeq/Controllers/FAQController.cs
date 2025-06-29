using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Rafeeq.DTOs;
using Rafeeq.DTOs.FAQ;
using Rafeeq.Services.FAQ;

namespace Rafeeq.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FAQController : ControllerBase
    {
        private readonly IFAQService _faqService;

        public FAQController(IFAQService faqService)
        {
            _faqService = faqService;
        }

       
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(PagedResult<FaqDto>), 200)] 
        public async Task<IActionResult> GetFAQ(
            [FromQuery] string? category = null,
            [FromQuery] string? searchQuery = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10) 
        {
            var pagedResult = await _faqService.GetActiveFAQAsync(category, searchQuery, pageNumber, pageSize);
            return Ok(pagedResult);
        }

        [HttpGet("categories")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(IEnumerable<FaqCategoryDto>), 200)]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _faqService.GetFAQCategoriesAsync();
            return Ok(categories);
        }

       
        [HttpPut("{id}/view")]
        [AllowAnonymous] 
        public async Task<IActionResult> IncrementFaqViewCount(int id)
        {
            await _faqService.IncrementFaqViewCountAsync(id);
            return NoContent();
        }

      
        [HttpPut("{id}/helpful")]
        [AllowAnonymous] 
        public async Task<IActionResult> IncrementFaqHelpfulCount(int id)
        {
            await _faqService.IncrementFaqHelpfulCountAsync(id);
            return NoContent();
        }

     
        [HttpPut("{id}/nothelpful")]
        [AllowAnonymous] 
        public async Task<IActionResult> IncrementFaqNotHelpfulCount(int id)
        {
            await _faqService.IncrementFaqNotHelpfulCountAsync(id);
            return NoContent();
        }
    }
}
