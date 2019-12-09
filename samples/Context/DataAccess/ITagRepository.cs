using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Models;

namespace SimpleBlog.DataAccess
{
    public interface ITagRepository
    {
        Task TryAddTagsAsync(
            IEnumerable<string> newTags, 
            CancellationToken cancellationToken = default);

        Task<IEnumerable<Tag>> GetTagsAsync(
            CancellationToken cancellationToken = default);
    }
}