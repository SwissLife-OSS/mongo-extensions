using System.Transactions;
using MongoDB.Driver;

namespace MongoDB.Extensions.Transactions
{
    public class MongoDbEnlistmentScope : IEnlistmentNotification
    {
        public delegate void Unregister();

        private readonly Unregister _unregister;
        private readonly IClientSessionHandle _sessionHandle;

        public MongoDbEnlistmentScope(IClientSessionHandle sessionHandle, Unregister unregister)
        {
            _sessionHandle = sessionHandle;
            _unregister = unregister;
        }

        public void Commit(Enlistment enlistment)
        {
            try
            {
                _sessionHandle.CommitTransaction();
                enlistment.Done();
            }
            finally
            {
                _unregister();
            }
        }

        public void InDoubt(Enlistment enlistment)
        {
            try
            {
                _sessionHandle.AbortTransaction();
                enlistment.Done();
            }
            finally
            {
                _unregister();
            }
        }

        public void Prepare(PreparingEnlistment preparingEnlistment)
        {
            preparingEnlistment.Prepared();
        }

        public void Rollback(Enlistment enlistment)
        {
            try
            {
                _sessionHandle.AbortTransaction();
                enlistment.Done();
            }
            finally
            {
                _unregister();
            }
        }
    }
}
