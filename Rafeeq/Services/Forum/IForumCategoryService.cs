using Rafeeq.DTOs.Forum;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rafeeq.Services.Forum
{
    public interface IForumCategoryService
    {
        Task<List<ForumCategoryDto>> GetAllCategoriesAsync();
        Task<ForumCategoryDto?> CreateCategoryAsync(CreateForumCategoryDto dto);
        Task<ForumCategoryDto?> UpdateCategoryAsync(int id, CreateForumCategoryDto dto); 
        Task<bool> DeleteCategoryAsync(int id); 
    }
}
