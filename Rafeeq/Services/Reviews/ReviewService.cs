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
    }
}
