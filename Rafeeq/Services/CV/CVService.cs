using AutoMapper;
using Rafeeq.DTOs.CV;
using Rafeeq.Models;
using Rafeeq.Repositories.CV;
using Rafeeq.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rafeeq.Services.CV
{
    public class CVService
    {
        private readonly UnitOfWorkManager _unitOfWork;
        private readonly IMapper _mapper;

        public CVService(UnitOfWorkManager unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        // Add a comment to a CV
        public async Task<(bool Success, string Message, CVCommentDto Data)> AddCommentAsync(AddCVCommentDto dto, int mentorId)
        {
            if (!await _unitOfWork.CVCommentRepository.DoesCVExistAsync(dto.CVId))
            {
                return (false, "CV not found", null);
            }

            var mentor = await _unitOfWork.UserRepository.GetByIdAsync(mentorId);
            if (mentor == null || !mentor.IsMentor.GetValueOrDefault())
            {
                return (false, "Only mentors can comment on CVs", null);
            }

            var comment = new CVComment
            {
                CVId = dto.CVId,
                MentorId = mentorId,
                Comment = dto.Comment,
                CreatedAt = DateTime.UtcNow
            };

            var addedComment = await _unitOfWork.CVCommentRepository.AddAsync(comment);

            var commentDto = _mapper.Map<CVCommentDto>(addedComment);
            commentDto.MentorName = mentor.FullName;

            return (true, "Comment added successfully", commentDto);
        }

        public async Task<(bool Success, string Message)> DeleteCommentAsync(int commentId, int currentUserId)
        {
            var comment = await _unitOfWork.CVCommentRepository.GetByIdAsync(commentId);
            if (comment == null)
            {
                return (false, "Comment not found");
            }

            var user = await _unitOfWork.UserRepository.GetUserWithRoleAsync(currentUserId);
            if (comment.MentorId != currentUserId && user?.Role?.RoleName != "Admin")
            {
                return (false, "You don't have permission to delete this comment");
            }

            var result = await _unitOfWork.CVCommentRepository.DeleteAsync(commentId);
            return result ? (true, "Comment deleted successfully") : (false, "Failed to delete comment");
        }
    }
}
