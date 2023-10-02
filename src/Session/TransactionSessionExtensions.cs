using System;
using MongoDB.Driver;

namespace MongoDB.Extensions.Session;

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

    public static IClientSessionHandle GetSessionHandle(
        this ISession session)
    {
        if (session is MongoSession mongoSession)
        {
            return mongoSession.Session;
        }

        throw new InvalidOperationException(
            $"Unknown session type {session.GetType().Name}");
    }
}
