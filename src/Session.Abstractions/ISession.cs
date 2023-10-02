using System;
using System.Threading;
using System.Threading.Tasks;

namespace MongoDB.Extensions.Session.Abstractions;

public interface ISession : IDisposable
{
    Task<T> WithTransactionAsync<T>(
        Func<ISession, CancellationToken, Task<T>> action,
        CancellationToken cancellationToken);
}
