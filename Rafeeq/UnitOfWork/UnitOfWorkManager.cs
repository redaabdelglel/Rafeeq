using Rafeeq.Models;
using Rafeeq.Repositories;
using Rafeeq.Repositories.Availability;
using Rafeeq.Repositories.Bookings;
using Rafeeq.Repositories.Chat;
using Rafeeq.Repositories.CV;
using Rafeeq.Repositories.Notifications;
using Rafeeq.Repositories.Payments;
using Rafeeq.Repositories.Reviews;
using Rafeeq.Repositories.Skills;
using Rafeeq.Repositories.Users;

namespace Rafeeq.UnitOfWork
{
    public class UnitOfWorkManager
    {
        public RafeeqContext context { get; }

        // Repository fields
        private UserRepository _userRepository;
        private SkillRepository _skillRepository;
        private AvailabilityRepository _availabilityRepository;
        private BookingRepository _bookingRepository;
        private PaymentRepository _paymentRepository;
        private ChatRepository _chatRepository;
        private ChatAttachmentRepository _chatAttachmentRepository;
        private ReviewRepository _reviewRepository;
        private NotificationRepository _notificationRepository;
        private MenteeCVRepository _menteeCVRepository;
        private CVCommentRepository _cvCommentRepository;

        public UnitOfWorkManager(RafeeqContext context)
        {
            this.context = context;
        }

        // Repositories with lazy loading
        public UserRepository UserRepository
        {
            get
            {
                if (_userRepository == null)
                {
                    _userRepository = new UserRepository(context);
                }
                return _userRepository;
            }
        }

        public SkillRepository SkillRepository
        {
            get
            {
                if (_skillRepository == null)
                {
                    _skillRepository = new SkillRepository(context);
                }
                return _skillRepository;
            }
        }

        public AvailabilityRepository AvailabilityRepository
        {
            get
            {
                if (_availabilityRepository == null)
                {
                    _availabilityRepository = new AvailabilityRepository(context);
                }
                return _availabilityRepository;
            }
        }

        public BookingRepository BookingRepository
        {
            get
            {
                if (_bookingRepository == null)
                {
                    _bookingRepository = new BookingRepository(context);
                }
                return _bookingRepository;
            }
        }

        public PaymentRepository PaymentRepository
        {
            get
            {
                if (_paymentRepository == null)
                {
                    _paymentRepository = new PaymentRepository(context);
                }
                return _paymentRepository;
            }
        }

        public ChatRepository ChatRepository
        {
            get
            {
                if (_chatRepository == null)
                {
                    _chatRepository = new ChatRepository(context);
                }
                return _chatRepository;
            }
        }

        public ChatAttachmentRepository ChatAttachmentRepository
        {
            get
            {
                if (_chatAttachmentRepository == null)
                {
                    _chatAttachmentRepository = new ChatAttachmentRepository(context);
                }
                return _chatAttachmentRepository;
            }
        }

        public ReviewRepository ReviewRepository
        {
            get
            {
                if (_reviewRepository == null)
                {
                    _reviewRepository = new ReviewRepository(context);
                }
                return _reviewRepository;
            }
        }

        public NotificationRepository NotificationRepository
        {
            get
            {
                if (_notificationRepository == null)
                {
                    _notificationRepository = new NotificationRepository(context);
                }
                return _notificationRepository;
            }
        }

        public MenteeCVRepository MenteeCVRepository
        {
            get
            {
                if (_menteeCVRepository == null)
                {
                    _menteeCVRepository = new MenteeCVRepository(context);
                }
                return _menteeCVRepository;
            }
        }

        public CVCommentRepository CVCommentRepository
        {
            get
            {
                if (_cvCommentRepository == null)
                {
                    _cvCommentRepository = new CVCommentRepository(context);
                }
                return _cvCommentRepository;
            }
        }

        public void Save()
        {
            context.SaveChanges();
        }

        public async Task<int> SaveAsync()
        {
            return await context.SaveChangesAsync();
        }
    }
}
