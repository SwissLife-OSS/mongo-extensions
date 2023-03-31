using System.Threading;
using System.Threading.Tasks;

namespace MongoDB.Extensions.Outbox.Core
{
    public interface IConsumption
    {
        Task ConsumeAsync(CancellationToken token);
    }
}
