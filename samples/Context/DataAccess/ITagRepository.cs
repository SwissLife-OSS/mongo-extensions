using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Models;

namespace SimpleBlog.DataAccess
{
    public interface ITagRepository
    {
        Task TryAddTagsAsync(IEnumerable<Tag> newTags, CancellationToken cancellationToken = default);
    }
}