using System.Collections.Concurrent;
using System.Transactions;
using MongoDB.Driver;

namespace MongoDB.Extensions.Transactions;

internal static class TransactionStore
{
    private static readonly ConcurrentDictionary<string, IClientSessionHandle>
        Sessions = new();

    public static bool TryGetSession(
        IMongoClient client,
        out IClientSessionHandle sessionHandle)
    {
        if (Transaction.Current?.TransactionInformation.LocalIdentifier is { } id)
        {
            sessionHandle = GetOrCreateTransaction(client, id);
            return true;
        }

        sessionHandle = null!;
        return false;
    }

    private static IClientSessionHandle GetOrCreateTransaction(
        IMongoClient mongoClient,
        string id)
    {
        return Sessions.GetOrAdd(id, CreateAndRegister);

        IClientSessionHandle CreateAndRegister(string idToRegister)
        {
            if (Transaction.Current is null)
            {
                throw new TransactionException(
                    "Cannot open a transaction without a valid scope");
            }

            IClientSessionHandle? session = mongoClient.StartSession();
            session.StartTransaction();
            MongoDbEnlistmentScope enlistment = new(session, Unregister);

            Transaction.Current.EnlistVolatile(enlistment, EnlistmentOptions.None);

            return session;

            void Unregister()
            {
                if (Sessions.TryRemove(idToRegister, out session))
                {
                    session.Dispose();
                }
            }
        }
    }
}
