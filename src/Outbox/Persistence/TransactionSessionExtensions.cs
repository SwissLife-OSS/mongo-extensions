using System;
using MongoDB.Driver;
using MongoDB.Extensions.Outbox.Core;

namespace MongoDB.Extensions.Outbox.Persistence
{
    public static class TransactionSessionExtensions
    {
        public static IClientSessionHandle GetSessionHandle(
            this ITransactionSession session)
        {
            if (session is MongoTransactionSession mongoTransactionSession)
            {
                return mongoTransactionSession.Session;
            }

            throw new InvalidOperationException(
                $"Unknown session type {session.GetType().Name}");
        }
    }
}
