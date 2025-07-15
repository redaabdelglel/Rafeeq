using Rafeeq.DTOs.Forum;
using Rafeeq.Models;
using Rafeeq.Repositories.Forum;
using AutoMapper;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rafeeq.Services.Forum
{
    public class ForumCategoryService : IForumCategoryService
    {
        private readonly IForumCategoryRepository _repo;
        private readonly IMapper _mapper;

        public ForumCategoryService(IForumCategoryRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<List<ForumCategoryDto>> GetAllCategoriesAsync()
        {
            var categories = await _repo.GetAllWithPostCountAsync();
            return categories.Select(c => new ForumCategoryDto
            {
                CategoryId = c.CategoryId,
                Name = c.Name,
                Description = c.Description,
                PostCount = c.Posts.Count(p => !p.IsDeleted)
            }).ToList();
        }

        public async Task<ForumCategoryDto?> CreateCategoryAsync(CreateForumCategoryDto dto)
        {
            var entity = new ForumCategory { Name = dto.Name, Description = dto.Description, CreatedAt = DateTime.UtcNow };
            await _repo.AddAsync(entity);
            await _repo.SaveAsync();
            return _mapper.Map<ForumCategoryDto>(entity);
        }

        public async Task<ForumCategoryDto?> UpdateCategoryAsync(int id, CreateForumCategoryDto dto)
        {
            var category = await _repo.GetByIdAsync(id);
            if (category == null) return null;
            category.Name = dto.Name;
            category.Description = dto.Description;
            _repo.Update(category);
            await _repo.SaveAsync();
            return _mapper.Map<ForumCategoryDto>(category);
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            var category = await _repo.GetByIdAsync(id);
            if (category == null) return false;
            _repo.Delete(category);
            await _repo.SaveAsync();
            return true;
        }
    }
}
