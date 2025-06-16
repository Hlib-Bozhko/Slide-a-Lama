
using System.Collections.Generic;
using System.Threading.Tasks;
using Datas.Entity;

namespace Datas.Service.Comments
{
    public interface ICommentService
    {
        // Async methods
        Task AddCommentAsync(Comment comment);
        Task<IList<Comment>> GetCommentsAsync();
        Task ResetCommentsAsync();

        // Standard methods 
        void AddComment(Comment comment);
        IList<Comment> GetComments();
        void ResetComments();
    }
}

