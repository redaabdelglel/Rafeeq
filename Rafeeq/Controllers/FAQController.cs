using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
        [ProducesResponseType(typeof(IEnumerable<FaqDto>), 200)]
        public async Task<IActionResult> GetFAQ([FromQuery] string? category = null)
        {
            var faqs = await _faqService.GetActiveFAQAsync(category);
            return Ok(faqs);
        }

  
        [HttpGet("categories")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(IEnumerable<FaqCategoryDto>), 200)]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _faqService.GetFAQCategoriesAsync();
            return Ok(categories);
        }
    }
}
