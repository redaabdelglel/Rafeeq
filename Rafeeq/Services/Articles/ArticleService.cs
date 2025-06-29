using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Rafeeq.DTOs.Articles;
using Rafeeq.UnitOfWork; 


namespace Rafeeq.Services.Articles
{
    public class ArticleService : IArticleService
    {
        private readonly UnitOfWorkManager _unitOfWork;
        private readonly IMapper _mapper;

        public ArticleService(UnitOfWorkManager unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<PagedResult<ArticleListDto>> GetPublishedArticlesAsync(
            string? category = null,
            string? searchQuery = null, 
            int pageNumber = 1,
            int pageSize = 6)
        {
            var query = _unitOfWork.ArticleRepository
                                   .GetPublishedArticlesQuery(category: category, searchQuery: searchQuery);

            var totalCount = await query.CountAsync();

            var articles = await query
                .OrderByDescending(a => a.CreatedAt) 
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var articleDtos = articles.Select(a => {
                var dto = _mapper.Map<ArticleListDto>(a);
                dto.AuthorName = a.Author?.FullName ?? "Unknown Author"; 
                return dto;
            }).ToList();

            return new PagedResult<ArticleListDto>
            {
                Items = articleDtos,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<ArticleDto?> GetArticleByIdAsync(int id)
        {
            var article = await _unitOfWork.ArticleRepository.GetByIdWithAuthorAsync(id);

            if (article == null || article.IsPublished == false)
            {
                return null;
            }

            var dto = _mapper.Map<ArticleDto>(article);
            dto.AuthorName = article.Author?.FullName ?? "Unknown Author"; 
            return dto;
        }

        public async Task IncrementArticleViewCountAsync(int id)
        {
            var article = await _unitOfWork.ArticleRepository.GetByIdAsync(id);
            if (article != null)
            {
                article.ViewCount = article.ViewCount + 1;
                await _unitOfWork.SaveAsync();
            }
        }
    }
}