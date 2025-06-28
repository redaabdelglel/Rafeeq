using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Rafeeq.DTOs.Articles;
using Rafeeq.Services.Articles;

namespace Rafeeq.Controllers
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
        [ProducesResponseType(typeof(IEnumerable<ArticleDto>), 200)]
        public async Task<IActionResult> GetArticles([FromQuery] string? category = null)
        {
            var articles = await _articleService.GetPublishedArticlesAsync(category);
            return Ok(articles);
        }


        [HttpGet("{id}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ArticleDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetArticle(int id)
        {
            var article = await _articleService.GetArticleByIdAsync(id);
            if (article == null)
            {
                return NotFound("Article not found or not published.");
            }

            _ = _articleService.IncrementArticleViewCountAsync(id);

            return Ok(article);
        }
    }
}
