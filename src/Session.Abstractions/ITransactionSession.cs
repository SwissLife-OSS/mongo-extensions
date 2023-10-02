using System;
using System.Threading.Tasks;

namespace MongoDB.Extensions.Session.Abstractions;

public interface ITransactionSession : IDisposable
{
    Task CommitAsync();
}
