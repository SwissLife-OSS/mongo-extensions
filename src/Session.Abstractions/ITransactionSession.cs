using System;
using System.Threading.Tasks;

namespace MongoDB.Extensions.Session;

public interface ITransactionSession : IDisposable
{
    Task CommitAsync();
}
