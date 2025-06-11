using Microsoft.EntityFrameworkCore;
using Rafeeq.Models;
using Rafeeq.Repositories;
using Rafeeq.Repositories.Auth;
using Rafeeq.Repositories.Availability;
using Rafeeq.Repositories.Bookings;
using Rafeeq.Repositories.Chat;
using Rafeeq.Repositories.CV;
using Rafeeq.Repositories.Mentee;
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
        private UserTokenRepository? _userTokenRepository; 

        private SkillRepository _skillRepository;
        private AvailabilityRepository _availabilityRepository;
        private MenteeBookingRepository _bookingRepository;
        private PaymentRepository _paymentRepository;
        private ChatRepository _chatRepository;
        private ChatAttachmentRepository _chatAttachmentRepository;
        private ReviewRepository _reviewRepository;
        private NotificationRepository _notificationRepository;
        private CVRepository _menteeCVRepository;
        private IMenteeRepository _menteeRepository;
        private CVCommentRepository _cvCommentRepository;
        private RoleRepository _roleRepository;


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
        public UserTokenRepository UserTokenRepository // Added for Auth
        {
            get
            {
                if (_userTokenRepository == null)
                {
                    _userTokenRepository = new UserTokenRepository(context);
                }
                return _userTokenRepository;
            }
        }


        public IMenteeRepository Mentees
        {
            get
            {
                if (_menteeRepository == null)
                {
                    _menteeRepository = new MenteeRepository(context);
                }
                return _menteeRepository;
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

        public MenteeBookingRepository BookingRepository
        {
            get
            {
                if (_bookingRepository == null)
                {
                    _bookingRepository = new MenteeBookingRepository(context);
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

        public CVRepository MenteeCVRepository
        {
            get
            {
                if (_menteeCVRepository == null)
                {
                    _menteeCVRepository = new CVRepository(context);
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
        public RoleRepository RoleRepository
        {
            get
            {
                if (_roleRepository == null)
                {
                    _roleRepository = new RoleRepository(context);
                }
                return _roleRepository;
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
