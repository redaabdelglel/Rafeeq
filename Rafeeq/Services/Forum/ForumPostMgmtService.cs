using Rafeeq.DTOs.Forum;
using Rafeeq.Models;
using Rafeeq.Repositories.Forum;
using AutoMapper;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rafeeq.Services.Forum
{
    public class ForumPostMgmtService : IForumPostMgmtService
    {
        private readonly IForumPostMgmtRepository _repo;
        private readonly IMapper _mapper;

        public ForumPostMgmtService(IForumPostMgmtRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<List<ForumPostDto>> GetAllAsync(int? categoryId = null, string? search = null, string? sortBy = "recent", bool? isSolved = null)
        {
            var posts = await _repo.GetAllAsync(categoryId, search, sortBy, isSolved);
            return posts.Select(_mapper.Map<ForumPostDto>).ToList();
        }

        public async Task<ForumPostDto?> GetByIdAsync(int postId)
        {
            var post = await _repo.GetByIdWithDetailsAsync(postId);
            return post == null ? null : _mapper.Map<ForumPostDto>(post);
        }

        public async Task<List<ForumPostDto>> GetByUserIdAsync(int userId)
        {
            var posts = await _repo.GetByUserIdAsync(userId);
            return posts.Select(_mapper.Map<ForumPostDto>).ToList();
        }

        public async Task<ForumPostDto> CreateAsync(CreateForumPostDto dto, int userId)
        {
            var post = _mapper.Map<ForumPost>(dto);
            post.UserId = userId;
            post.CreatedAt = DateTime.UtcNow;
            post.IsDeleted = false;
            post.Upvotes = 0;
            post.IsSolved = false;
            await _repo.AddAsync(post);
            await _repo.SaveAsync();
            return _mapper.Map<ForumPostDto>(post);
        }

        public async Task<bool> UpdateAsync(int postId, UpdateForumPostDto dto, int userId)
        {
            var post = await _repo.GetByIdWithDetailsAsync(postId);
            if (post == null || post.UserId != userId || post.IsDeleted) return false;
            post.Title = dto.Title;
            post.Content = dto.Content;
            post.CategoryId = dto.CategoryId;
            post.UpdatedAt = DateTime.UtcNow;
            _repo.Update(post);
            await _repo.SaveAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int postId, int userId)
        {
            var post = await _repo.GetByIdWithDetailsAsync(postId);
            if (post == null || post.UserId != userId || post.IsDeleted) return false;
            post.IsDeleted = true;
            _repo.Update(post);
            await _repo.SaveAsync();
            return true;
        }

        public async Task<bool> UpvoteAsync(int postId, int userId)
        {
            var post = await _repo.GetByIdWithDetailsAsync(postId);
            if (post == null || post.IsDeleted) return false;
            var upvote = await _repo.GetUserUpvoteAsync(postId, userId);
            if (upvote != null) return false; // already upvoted
            var newUpvote = new ForumPostUpvote { PostId = postId, UserId = userId, CreatedAt = DateTime.UtcNow };
            await _repo.AddUpvoteAsync(newUpvote);
            post.Upvotes += 1;
            _repo.Update(post);
            await _repo.SaveAsync();
            return true;
        }

        public async Task<bool> RemoveUpvoteAsync(int postId, int userId)
        {
            var post = await _repo.GetByIdWithDetailsAsync(postId);
            if (post == null || post.IsDeleted) return false;
            var upvote = await _repo.GetUserUpvoteAsync(postId, userId);
            if (upvote == null) return false;
            _repo.RemoveUpvote(upvote);
            post.Upvotes = Math.Max(0, post.Upvotes - 1);
            _repo.Update(post);
            await _repo.SaveAsync();
            return true;
        }

        public async Task<bool> MarkAsSolvedAsync(int postId, int userId)
        {
            var post = await _repo.GetByIdWithDetailsAsync(postId);
            if (post == null || post.UserId != userId || post.IsDeleted) return false;
            post.IsSolved = true;
            _repo.Update(post);
            await _repo.SaveAsync();
            return true;
        }
    }
}
