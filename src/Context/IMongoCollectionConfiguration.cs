namespace MongoDB.Extensions.Context
{
    public interface IMongoCollectionConfiguration<TDocument> where TDocument : class
    {
        void Configure(IMongoCollectionBuilder<TDocument> mongoCollectionBuilder);
    }
}
