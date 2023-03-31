namespace MongoDB.Extensions.Outbox.Persistence
{
    public class DatabaseOptions
    {
        public DatabaseOptions(string connectionString, string databaseName)
        {
            ConnectionString = connectionString;
            DatabaseName = databaseName;
        }

        public string ConnectionString { get; }
        public string DatabaseName { get; }
    }
}
