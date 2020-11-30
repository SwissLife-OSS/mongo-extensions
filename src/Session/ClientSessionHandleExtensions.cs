using System;
using MongoDB.Driver;

namespace MongoDB.Extensions.Session
{
    public static class ClientSessionHandleExtensions
    {
        public static Guid GetSessionId(this IClientSessionHandle clientSessionHandle)
        {
            return clientSessionHandle.ServerSession.Id["id"].AsGuid;
        }
    }
}
