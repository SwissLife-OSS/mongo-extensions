using System;
using System.Threading;
using System.Threading.Tasks;
using Models;

namespace SimpleBlog.DataAccess
{
    public interface IUserRepository
    {
        Task AddUserAsync(User user, CancellationToken cancellationToken = default);
        Task AttachBlogToUserAsync(string userId, Guid blogId, CancellationToken cancellationToken = default);
    }
}