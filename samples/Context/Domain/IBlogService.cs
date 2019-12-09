using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Models;

namespace SimpleBlog.Domain
{
    public interface IBlogService
    {
        Task PostBlogAsync(BlogPost blogPost, CancellationToken cancellationToken);
        Task<IEnumerable<Blog>> GetAllBlogsAsync(CancellationToken cancellationToken);
        Task<IEnumerable<Tag>> GetAllTagsAsync(CancellationToken cancellationToken);
    }
}