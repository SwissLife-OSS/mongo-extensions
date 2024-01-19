using System;
using MongoDB.Driver;

namespace MongoDB.Extensions.Context
{
    public static class DefaultDefinitions
    {
        public static readonly TransactionOptions DefaultTransactionOptions =
            new TransactionOptions(
                ReadConcern.Majority,
                ReadPreference.Primary,
                WriteConcern.WMajority.With(journal: true),
                TimeSpan.FromSeconds(60));
    }
}
