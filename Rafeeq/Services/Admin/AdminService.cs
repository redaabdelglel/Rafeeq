using Rafeeq.UnitOfWork;

namespace Rafeeq.Services.Admin
{
    public class AdminService
    {
        private readonly UnitOfWorkManager _unitOfWork;

        public AdminService(UnitOfWorkManager _unitOfWork)
        {
            this._unitOfWork = _unitOfWork;
        }





    }
}
