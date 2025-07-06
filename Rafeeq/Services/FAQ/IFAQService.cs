using Rafeeq.DTOs;
using Rafeeq.DTOs.FAQ;

namespace Rafeeq.Services.FAQ
{
    public interface IFAQService
    {
        Task<PagedResult<FaqDto>> GetActiveFAQAsync(
           string? category = null,
           string? searchQuery = null, 
           int pageNumber = 1,      
           int pageSize = 10);    

        Task<IEnumerable<FaqCategoryDto>> GetFAQCategoriesAsync();

        Task IncrementFaqViewCountAsync(int faqId);

        Task IncrementFaqHelpfulCountAsync(int faqId);

        Task IncrementFaqNotHelpfulCountAsync(int faqId);




        // Admin-specific CRUD methods
        Task<PagedResult<FaqDto>> GetAllFaqsForAdminAsync( 
            string? category = null,
            string? searchQuery = null,
            int pageNumber = 1,
            int pageSize = 10);
        Task<FaqDto?> GetFaqByIdForAdminAsync(int faqId); 
        Task<FaqDto> CreateFaqAsync(FaqCreateDto faqDto);
        Task<FaqDto?> UpdateFaqAsync(int id, FaqUpdateDto faqDto);
        Task<bool> DeleteFaqAsync(int id);
    }
}
