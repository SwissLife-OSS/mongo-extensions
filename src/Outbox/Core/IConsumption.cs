using System.Threading;
using System.Threading.Tasks;

namespace SwissLife.MongoDB.Extensions.Outbox.Core
{
    public interface IConsumption
    {
        Task ConsumeAsync(CancellationToken token);
    }
}
