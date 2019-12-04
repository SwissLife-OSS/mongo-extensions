namespace MongoDB.Extensions.Context
{
    public interface IMongoCollectionConfiguration<TDocument> where TDocument : class
    {
        void OnConfiguring(IMongoCollectionBuilder<TDocument> mongoCollectionBuilder);
    }
}
