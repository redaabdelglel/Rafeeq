using Rafeeq.Models;
using Rafeeq.Repositories.Bookings;
using Rafeeq.Repositories.CV;
using Rafeeq.Repositories.Users;

namespace Rafeeq.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        IMenteeBookingRepository Bookings { get; }
        ICVRepository CVs { get; }
        IMentorRepository Mentors { get; }
        Task<int> CompleteAsync();
    }

    public class CVBookingUnitOfWork : IUnitOfWork
    {
        private readonly RafeeqContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<CVBookingUnitOfWork> _logger;
        private readonly ILoggerFactory _loggerFactory;
        private bool _disposed = false;

        public CVBookingUnitOfWork(
            RafeeqContext context,
            IWebHostEnvironment environment,
            ILogger<CVBookingUnitOfWork> logger,
            ILoggerFactory loggerFactory)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));

            // Initialize repositories with their respective loggers
            Bookings = new MenteeBookingRepository(_context, _loggerFactory.CreateLogger<MenteeBookingRepository>());
            CVs = new MenteeCVRepository(_context, _environment);
            Mentors = new MenteeMentorRepository(_context);
        }


        public IMenteeBookingRepository Bookings { get; }
        public ICVRepository CVs { get; }
        public IMentorRepository Mentors { get; }

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}