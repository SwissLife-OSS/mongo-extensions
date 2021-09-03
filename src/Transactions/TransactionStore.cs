using System.Collections.Concurrent;
using System.Transactions;
using MongoDB.Driver;

namespace MongoDB.Extensions.Transactions
{
    internal static class TransactionStore
    {
        private static readonly ConcurrentDictionary<string, IClientSessionHandle>
            _sessions = new();

        private static IClientSessionHandle GetOrCreateTransaction(string id) =>
            _sessions.GetOrAdd(id, CreateAndRegister);

        private static IClientSessionHandle CreateAndRegister(string id)
        {
            if (Transaction.Current is null)
            {
                throw new TransactionException("Cannot open a transaction without a valid scope");
            }

            IClientSessionHandle? session = _client.StartSession();
            session.StartTransaction();
            MongoDbEnlistmentScope enlistment = new(session, Unregister);

            Transaction.Current.EnlistVolatile(enlistment, EnlistmentOptions.None);

            return session;

            void Unregister()
            {
                if (_sessions.TryRemove(id, out session))
                {
                    session.Dispose();
                }
            }
        }

        public static bool TryGetSession(out IClientSessionHandle sessionHandle)
        {
            if (Transaction.Current?.TransactionInformation.LocalIdentifier is { } id)
            {
                sessionHandle = GetOrCreateTransaction(id);
                return true;
            }

            sessionHandle = null!;
            return false;
        }
    }
}
