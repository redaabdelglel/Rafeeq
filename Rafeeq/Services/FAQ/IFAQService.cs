using Rafeeq.DTOs.FAQ;

namespace Rafeeq.Services.FAQ
{
    public interface IFAQService
    {
        Task<IEnumerable<FaqDto>> GetActiveFAQAsync(string? category = null);
        Task<IEnumerable<FaqCategoryDto>> GetFAQCategoriesAsync();
    }
}
