using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Rafeeq.DTOs;
using Rafeeq.DTOs.Articles;
using Rafeeq.Models;
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

     
        public async Task<ArticleDto> CreateArticleAsync(ArticleCreateDto articleDto)
        {
            var article = _mapper.Map<Article>(articleDto);
            article.CreatedAt = DateTime.UtcNow;
            article.UpdatedAt = DateTime.UtcNow;
            article.ViewCount = 0;

            _unitOfWork.ArticleRepository.Add(article);
            await _unitOfWork.SaveAsync();

            var createdArticleWithAuthor = await _unitOfWork.ArticleRepository.GetByIdWithAuthorAsync(article.ArticleId);
            if (createdArticleWithAuthor == null)
            {
                throw new InvalidOperationException("Failed to retrieve newly created article.");
            }
            var dto = _mapper.Map<ArticleDto>(createdArticleWithAuthor);
            dto.AuthorName = createdArticleWithAuthor.Author?.FullName ?? "Unknown Author";
            return dto;
        }

        public async Task<ArticleDto?> UpdateArticleAsync(int id, ArticleUpdateDto articleDto)
        {
            var article = await _unitOfWork.ArticleRepository.GetByIdAsync(id);
            if (article == null)
            {
                return null;
            }

            _mapper.Map(articleDto, article); 
            article.UpdatedAt = DateTime.UtcNow; 

            _unitOfWork.ArticleRepository.Update(article); 
            await _unitOfWork.SaveAsync();

            var updatedArticleWithAuthor = await _unitOfWork.ArticleRepository.GetByIdWithAuthorAsync(article.ArticleId);
            if (updatedArticleWithAuthor == null)
            {
                throw new InvalidOperationException("Failed to retrieve updated article.");
            }
            var dto = _mapper.Map<ArticleDto>(updatedArticleWithAuthor);
            dto.AuthorName = updatedArticleWithAuthor.Author?.FullName ?? "Unknown Author";
            return dto;
        }

        public async Task<bool> DeleteArticleAsync(int id)
        {
            var article = await _unitOfWork.ArticleRepository.GetByIdAsync(id);
            if (article == null)
            {
                return false;
            }

            _unitOfWork.ArticleRepository.Remove(article);
            await _unitOfWork.SaveAsync();
            return true;
        }

        public async Task<PagedResult<ArticleListDto>> GetAllArticlesForAdminAsync(
              string? category = null,
              string? searchQuery = null,
              int pageNumber = 1,
              int pageSize = 10)
        {
            IQueryable<Article> query = _unitOfWork.ArticleRepository.GetQuery();

       
            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(a => a.Category == category);
            }

            if (!string.IsNullOrEmpty(searchQuery))
            {
                var lowerCaseQuery = searchQuery.ToLower();
                query = query.Where(a =>
                    a.Title.ToLower().Contains(lowerCaseQuery) ||
                    (a.Summary != null && a.Summary.ToLower().Contains(lowerCaseQuery)) ||
                    a.Content.ToLower().Contains(lowerCaseQuery)
                );
            }

            query = query.Include(a => a.Author);

            var totalCount = await query.CountAsync();

            var articles = await query
                .OrderByDescending(a => a.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var articleDtos = articles.Select(a => {
                var dto = _mapper.Map<ArticleListDto>(a);
                dto.AuthorName = a.Author?.FullName ?? "Unknown Author";
                dto.IsPublished = a.IsPublished;
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

        public async Task<ArticleDto?> GetArticleByIdForAdminAsync(int id)
        {
            var article = await _unitOfWork.ArticleRepository.GetByIdWithAuthorAsync(id);
            if (article == null)
            {
                return null;
            }
            var dto = _mapper.Map<ArticleDto>(article);
            dto.AuthorName = article.Author?.FullName ?? "Unknown Author";
            return dto;
        }
    }
}
