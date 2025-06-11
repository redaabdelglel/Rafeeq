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
            // Check if the CV exists
            if (!await _unitOfWork.CVCommentRepository.DoesCVExistAsync(dto.CVId))
            {
                return (false, "CV not found", null);
            }

            // Check if the user is a mentor
            var mentor = await _unitOfWork.UserRepository.GetByIdAsync(mentorId);
            if (mentor == null || !mentor.IsMentor.GetValueOrDefault())
            {
                return (false, "Only mentors can comment on CVs", null);
            }

            // Create new comment
            var comment = new CVComment
            {
                CVId = dto.CVId,
                MentorId = mentorId,
                Comment = dto.Comment,
                CreatedAt = DateTime.UtcNow
            };

            // Add to database
            var addedComment = await _unitOfWork.CVCommentRepository.AddAsync(comment);

            // Map to DTO with mentor name
            var commentDto = _mapper.Map<CVCommentDto>(addedComment);
            commentDto.MentorName = mentor.FullName;

            return (true, "Comment added successfully", commentDto);
        }

        // Delete a comment
        public async Task<(bool Success, string Message)> DeleteCommentAsync(int commentId, int currentUserId)
        {
            // Get the comment
            var comment = await _unitOfWork.CVCommentRepository.GetByIdAsync(commentId);
            if (comment == null)
            {
                return (false, "Comment not found");
            }

            // Check if user is comment author or admin
            var user = await _unitOfWork.UserRepository.GetUserWithRoleAsync(currentUserId);
            if (comment.MentorId != currentUserId && user?.Role?.RoleName != "Admin")
            {
                return (false, "You don't have permission to delete this comment");
            }

            // Delete the comment
            var result = await _unitOfWork.CVCommentRepository.DeleteAsync(commentId);
            return result ? (true, "Comment deleted successfully") : (false, "Failed to delete comment");
        }
    }
}
