using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Rafeeq.Services.Articles; 
using System;


using Microsoft.AspNetCore.Authorization;

namespace Rafeeq.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ArticlesController : ControllerBase
    {
        private readonly IArticleService _articleService;

        public ArticlesController(IArticleService articleService)
        {
            _articleService = articleService;
        }

     
        [HttpGet]
        [AllowAnonymous] 
        public async Task<IActionResult> GetArticles(
            [FromQuery] string? category = null,
            [FromQuery] string? searchQuery = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 6)
        {
            var pagedResult = await _articleService.GetPublishedArticlesAsync(
                category: category,
                searchQuery: searchQuery,
                pageNumber: pageNumber,
                pageSize: pageSize);

            return Ok(pagedResult);
        }

        [HttpGet("{id}")]
        [AllowAnonymous] 
        public async Task<IActionResult> GetArticle(int id)
        {
            var articleDto = await _articleService.GetArticleByIdAsync(id);

            if (articleDto == null)
            {
                return NotFound();
            }

            return Ok(articleDto);
        }

        [HttpPut("{id}/view")]
        [AllowAnonymous]

        public async Task<IActionResult> IncrementViewCount(int id)
        {
            await _articleService.IncrementArticleViewCountAsync(id);
            return NoContent();
        }

    
    }
}