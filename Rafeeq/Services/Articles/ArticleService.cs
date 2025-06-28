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

        public async Task<IEnumerable<ArticleDto>> GetPublishedArticlesAsync(string? category = null)
        {
            var articles = await _unitOfWork.ArticleRepository
                                            .GetPublishedArticlesQuery(category: category)
                                            .ToListAsync();

            return articles.Select(a => {
                var dto = _mapper.Map<ArticleDto>(a);
                dto.AuthorName = a.Author?.FullName ?? "Unknown Author";
                return dto;
            }).ToList();
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
                _unitOfWork.ArticleRepository.Update(article);
                await _unitOfWork.SaveAsync();
            }
        }
    }
}
