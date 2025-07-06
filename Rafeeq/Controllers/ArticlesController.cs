using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Rafeeq.Services.Articles; 
using System;


using Microsoft.AspNetCore.Authorization;
using AutoMapper;
using Rafeeq.UnitOfWork;
using Rafeeq.DTOs.Articles;

namespace Rafeeq.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ArticlesController : ControllerBase
    {
        private readonly IArticleService _articleService;
        private readonly IMapper _mapper; 
        private readonly UnitOfWorkManager _unitOfWork;

        public ArticlesController(IArticleService articleService, IMapper mapper, UnitOfWorkManager unitOfWork)
        {
            _articleService = articleService;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
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


        //Admin CRUD Endpoints
        [HttpGet("admin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllArticlesForAdmin(
            [FromQuery] string? category = null,
            [FromQuery] string? searchQuery = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var pagedResult = await _articleService.GetAllArticlesForAdminAsync(
                category: category,
                searchQuery: searchQuery,
                pageNumber: pageNumber,
                pageSize: pageSize);
            return Ok(pagedResult);
        }

        [HttpGet("admin/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetArticleForAdmin(int id)
        {
            var articleDto = await _articleService.GetArticleByIdForAdminAsync(id);
            if (articleDto == null)
            {
                return NotFound("Article not found.");
            }
            return Ok(articleDto);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateArticle([FromBody] ArticleCreateDto articleDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var createdArticle = await _articleService.CreateArticleAsync(articleDto);
            return CreatedAtAction(nameof(GetArticleForAdmin), new { id = createdArticle.ArticleId }, createdArticle);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateArticle(int id, [FromBody] ArticleUpdateDto articleDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var updatedArticle = await _articleService.UpdateArticleAsync(id, articleDto);
            if (updatedArticle == null)
            {
                return NotFound($"Article with ID {id} not found.");
            }
            return Ok(updatedArticle);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteArticle(int id)
        {
            var result = await _articleService.DeleteArticleAsync(id);
            if (!result)
            {
                return NotFound($"Article with ID {id} not found.");
            }
            return NoContent(); 
        }

    }
}