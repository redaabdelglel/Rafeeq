using Rafeeq.DTOs.Reviews;
using Rafeeq.UnitOfWork;

namespace Rafeeq.Services.Reviews
{
    public class ReviewService
    {
        private readonly UnitOfWorkManager unitOfWork;

        public ReviewService(UnitOfWorkManager unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<ReviewDto>> GetReviewsForMentorAsync(int mentorId)
        {
            return await unitOfWork.ReviewRepository.GetReviewsForMentorAsync(mentorId);
        }

    }
}
