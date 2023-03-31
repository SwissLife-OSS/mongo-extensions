using System;
using System.Threading.Tasks;

namespace SwissLife.MongoDB.Extensions.Outbox.Core
{
    public interface ITransactionSession : IDisposable
    {
        Task CommitAsync();
    }
}
