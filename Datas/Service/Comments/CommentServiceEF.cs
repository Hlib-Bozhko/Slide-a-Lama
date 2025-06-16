
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Datas.Entity;
using Datas.Service.Comments;

namespace Datas.Service.Comments
{
    public class CommentServiceEF : ICommentService
    {
        private readonly SlideALamaDbContext _context;
        private readonly ILogger<CommentServiceEF> _logger;

        public CommentServiceEF(SlideALamaDbContext context, ILogger<CommentServiceEF> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task AddCommentAsync(Comment comment)
        {
            try
            {
                if (comment == null)
                    throw new ArgumentNullException(nameof(comment));

                if (string.IsNullOrWhiteSpace(comment.Name))
                    throw new ArgumentException("Comment name cannot be empty", nameof(comment));

                if (string.IsNullOrWhiteSpace(comment.Text))
                    throw new ArgumentException("Comment text cannot be empty", nameof(comment));

                if (comment.Text.Length > 1000)
                    throw new ArgumentException("Comment text is too long (max 1000 characters)", nameof(comment));

                _context.Comments.Add(comment);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Comment added by user {Name}: {Text}", 
                    comment.Name, comment.Text.Substring(0, Math.Min(50, comment.Text.Length)));
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while adding comment by user {Name}", comment?.Name);
                throw new InvalidOperationException("Failed to save comment to database", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while adding comment by user {Name}", comment?.Name);
                throw;
            }
        }

        public async Task<IList<Comment>> GetCommentsAsync()
        {
            try
            {
                var comments = await _context.Comments
                    .OrderByDescending(c => c.Text)
                    .ToListAsync();
                
                _logger.LogInformation("Retrieved {Count} comments", comments.Count);
                return comments;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving comments");
                throw new InvalidOperationException("Failed to retrieve comments", ex);
            }
        }

        public async Task ResetCommentsAsync()
        {
            try
            {
                var allComments = await _context.Comments.ToListAsync();
                _context.Comments.RemoveRange(allComments);
                await _context.SaveChangesAsync();
                
                _logger.LogWarning("All comments have been reset");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting comments");
                throw new InvalidOperationException("Failed to reset comments", ex);
            }
        }

        // Synchronous methods for backwards compatibility
        public void AddComment(Comment comment)
        {
            AddCommentAsync(comment).GetAwaiter().GetResult();
        }

        public IList<Comment> GetComments()
        {
            return GetCommentsAsync().GetAwaiter().GetResult();
        }

        public void ResetComments()
        {
            ResetCommentsAsync().GetAwaiter().GetResult();
        }
    }
}

