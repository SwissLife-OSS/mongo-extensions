using System.Threading;
using System.Threading.Tasks;
using Models;

namespace SimpleBlog.Domain
{
    public interface IBlogService
    {
        Task PostBlogAsync(BlogPost blogPost, CancellationToken cancellationToken);
    }
}