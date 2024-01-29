using System;
using System.Threading;
using System.Threading.Tasks;

namespace MongoDB.Extensions.Session;

public interface ISession : IDisposable
{
    Task<T> WithTransactionAsync<T>(
        Func<ISession, CancellationToken, Task<T>> action,
        CancellationToken cancellationToken);

    ITransactionSession StartTransaction(
        CancellationToken cancellationToken);
}
