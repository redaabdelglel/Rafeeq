using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Rafeeq.DTOs.FAQ;
using Rafeeq.UnitOfWork;

namespace Rafeeq.Services.FAQ
{
    public class FAQService : IFAQService
    {
        private readonly UnitOfWorkManager _unitOfWork;
        private readonly IMapper _mapper;

        public FAQService(UnitOfWorkManager unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<FaqDto>> GetActiveFAQAsync(string? category = null)
        {
            var faqs = await _unitOfWork.FAQRepository.GetActiveFAQQuery(category).ToListAsync();
            return _mapper.Map<IEnumerable<FaqDto>>(faqs);
        }

        public async Task<IEnumerable<FaqCategoryDto>> GetFAQCategoriesAsync()
        {
            var categoriesWithCount = await _unitOfWork.FAQRepository.GetQuery()
                                                                      .Where(f => f.IsActive)
                                                                      .GroupBy(f => f.Category)
                                                                      .Select(g => new FaqCategoryDto
                                                                      {
                                                                          CategoryName = g.Key ?? "Uncategorized",
                                                                          QuestionCount = g.Count()
                                                                      })
                                                                      .ToListAsync();
            return categoriesWithCount;
        }
    }
}
