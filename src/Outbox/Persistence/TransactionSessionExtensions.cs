using System;
using MongoDB.Driver;
using SwissLife.MongoDB.Extensions.Outbox.Core;

namespace SwissLife.MongoDB.Extensions.Outbox.Persistence
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
