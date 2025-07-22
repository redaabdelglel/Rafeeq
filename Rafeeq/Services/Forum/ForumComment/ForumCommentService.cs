using AutoMapper;
using Rafeeq.DTOs.ForumComment;
using Rafeeq.Models; 
using Rafeeq.Services.Forum.ForumComment;
using Rafeeq.UnitOfWork;
using System.Linq;
using System.Threading.Tasks;

namespace Rafeeq.Services.Forum
{
    public class ForumCommentService : IForumCommentService
    {
        private readonly UnitOfWorkManager _unitOfWork;
        private readonly IMapper _mapper;

        public ForumCommentService(UnitOfWorkManager unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ForumCommentDto?> CreateForumCommentAsync(int postId, int userId, CreateForumCommentDto dto)
        {
            var post = await _unitOfWork.ForumPostRepository.GetByIdAsync(postId);
            if (post == null || post.IsDeleted == true) return null;

            var comment = _mapper.Map<Rafeeq.Models.ForumComment>(dto);
            comment.PostId = postId;
            comment.UserId = userId;
            comment.CreatedAt = DateTime.Now;
            comment.IsDeleted = false;

            _unitOfWork.ForumCommentRepository.Add(comment);
            await _unitOfWork.SaveAsync();

            var createdComment = await _unitOfWork.ForumCommentRepository.GetCommentByIdWithUserAsync(comment.CommentId);
            if (createdComment == null) return null;

            return _mapper.Map<ForumCommentDto>(createdComment);
        }

        public async Task<bool> UpdateForumCommentAsync(int commentId, int currentUserId, UpdateForumCommentDto dto)
        {
            var comment = await _unitOfWork.ForumCommentRepository.GetCommentByIdWithUserAsync(commentId); // The error was in accessing .UserId directly on the nullable type here.
            if (comment == null || comment.IsDeleted == true) return false; // Check for null before accessing properties
            if (comment.UserId != currentUserId) return false;

            _mapper.Map(dto, comment);
            _unitOfWork.ForumCommentRepository.Update(comment);
            await _unitOfWork.SaveAsync();
            return true;
        }

        public async Task<bool> DeleteForumCommentAsync(int commentId, int currentUserId)
        {
            var comment = await _unitOfWork.ForumCommentRepository.GetCommentByIdWithUserAsync(commentId);
            if (comment == null || comment.IsDeleted == true) return false; // Check for null
            if (comment.UserId != currentUserId) return false;

            comment.IsDeleted = true;
            _unitOfWork.ForumCommentRepository.Update(comment);
            await _unitOfWork.SaveAsync();
            return true;
        }
        public async Task<List<ForumCommentDto>> GetCommentsByPostAsync(int postId)
        {
            var comments = _unitOfWork.ForumCommentRepository
                .GetCommentsByPostQuery(postId)
                .ToList();

            return comments.Select(c => _mapper.Map<ForumCommentDto>(c)).ToList();
        }

    }
}