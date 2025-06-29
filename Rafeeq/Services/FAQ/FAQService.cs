using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Rafeeq.DTOs;
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

        public async Task<PagedResult<FaqDto>> GetActiveFAQAsync(
            string? category = null,
            string? searchQuery = null,
            int pageNumber = 1,
            int pageSize = 10)
        {
            var query = _unitOfWork.FAQRepository.GetActiveFAQQuery(category, searchQuery);

            var totalCount = await query.CountAsync();

            var faqs = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var faqDtos = _mapper.Map<IEnumerable<FaqDto>>(faqs);

            return new PagedResult<FaqDto>
            {
                Items = faqDtos.ToList(), 
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
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

        public async Task IncrementFaqViewCountAsync(int faqId)
        {
            var faq = await _unitOfWork.FAQRepository.GetFaqByIdAsync(faqId);
            if (faq != null)
            {
                faq.ViewCount++;
                _unitOfWork.FAQRepository.Update(faq); 
                await _unitOfWork.SaveAsync();
            }
        }

        public async Task IncrementFaqHelpfulCountAsync(int faqId)
        {
            var faq = await _unitOfWork.FAQRepository.GetFaqByIdAsync(faqId);
            if (faq != null)
            {
                faq.HelpfulCount++;
                _unitOfWork.FAQRepository.Update(faq);
                await _unitOfWork.SaveAsync();
            }
        }

        public async Task IncrementFaqNotHelpfulCountAsync(int faqId)
        {
            var faq = await _unitOfWork.FAQRepository.GetFaqByIdAsync(faqId);
            if (faq != null)
            {
                faq.NotHelpfulCount++;
                _unitOfWork.FAQRepository.Update(faq);
                await _unitOfWork.SaveAsync();
            }
        }
    }
}
