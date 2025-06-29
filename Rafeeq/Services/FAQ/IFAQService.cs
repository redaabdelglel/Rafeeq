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
    }
}
