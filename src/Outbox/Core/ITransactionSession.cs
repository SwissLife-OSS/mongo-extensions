using System;
using System.Threading.Tasks;

namespace MongoDB.Extensions.Outbox.Core
{
    public interface ITransactionSession : IDisposable
    {
        Task CommitAsync();
    }
}
