using System;
using MongoDB.Driver;

namespace MongoDB.Extensions.Session
{
    public static class ClientSessionHandleExtensions
    {
        [Obsolete("This method has been moved to MongoDB.Prime.Extensions")]
        public static Guid GetSessionId(this IClientSessionHandle clientSessionHandle)
        {
            return clientSessionHandle.ServerSession.Id["id"].AsGuid;
        }
    }
}
