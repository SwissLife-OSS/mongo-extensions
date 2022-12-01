using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Driver;

namespace MongoDB.Prime.Extensions;

public static class MongoDatabaseExtensions
{
    public static void EnableProfiling(
        this IMongoDatabase mongoDatabase,
        ProfileLevel profileLevel = ProfileLevel.All)
    {
        var profileCommand = new BsonDocument("profile", (int)profileLevel);

        mongoDatabase.RunCommand<BsonDocument>(profileCommand);
    }

    public static ProfilingStatus GetProfilingStatus(
        this IMongoDatabase mongoDatabase)
    {
        var profileStatusCommand = new BsonDocument("profile", -1);

        BsonDocument profileBsonDocument =
            mongoDatabase.RunCommand<BsonDocument>(profileStatusCommand);

        return CreateProfilingStatus(profileBsonDocument);
    }

    private static ProfilingStatus CreateProfilingStatus(
        BsonDocument profileBsonDocument)
    {
        return new ProfilingStatus(
            level: (ProfileLevel)profileBsonDocument["was"].AsInt32,
            slowMs: profileBsonDocument["slowms"].AsInt32,
            sampleRate: profileBsonDocument["sampleRate"].AsDouble,
            filter: profileBsonDocument["ok"].AsDouble.ToString());
    }

    public static IEnumerable<string> GetProfiledOperations(
        this IMongoDatabase mongoDatabase)
    {
        IMongoCollection<BsonDocument> collection = mongoDatabase
            .GetCollection<BsonDocument>("system.profile");

        List<BsonDocument> docs = collection
            .Find(new BsonDocument())
            .ToList();

        IEnumerable<string> jsons = docs.Select(bson => bson.ToJson(
            new JsonWriterSettings
            {
                OutputMode = JsonOutputMode.RelaxedExtendedJson
            }));

        IEnumerable<string> normalizedJson = jsons
            .Select(json => JsonSerializer
                .Serialize(
                    JsonDocument.Parse(json).RootElement,
                    new JsonSerializerOptions()
                    {
                        WriteIndented = true
                    }));

        return normalizedJson;
    }

    /// <summary>
    /// Gets the collection by Type name. The name of the collection
    /// will be the typeof(TDocument).Name.
    /// </summary>
    /// <typeparam name="TDocument">The type of document.</typeparam>
    /// <param name="mongoDatabase">The mongo database.</param>
    /// <param name="settings">The mongo collection settings.</param>
    /// <returns>The mongo collection with the name of the document type.</returns>
    public static IMongoCollection<TDocument> GetCollection<TDocument>(
        this IMongoDatabase mongoDatabase,
        MongoCollectionSettings? settings = null)
    {
        return mongoDatabase
            .GetCollection<TDocument>(typeof(TDocument).Name, settings);
    }

    /// <summary>
    /// Deletes all entries of every collection of the mongo database.
    /// The collections will NOT be dropped and the indexes stay unmodified.
    /// </summary>
    /// <param name="mongoDatabase">The database to clean the collections.</param>
    public static void CleanAllCollections(
        this IMongoDatabase mongoDatabase)
    {
        foreach (var name in mongoDatabase.ListCollectionNames().ToList())
        {
            IMongoCollection<BsonDocument> collection =
                mongoDatabase.GetCollection<BsonDocument>(name);

            collection.CleanCollection();
        }
    }

    /// <summary>
    /// Deletes all entries of every collection of the mongo database.
    /// The collections will NOT be dropped and the indexes stay unmodified.
    /// </summary>
    /// <param name="mongoDatabase">The database to clean the collections.</param>
    public static async Task CleanAllCollectionsAsync(
        this IMongoDatabase mongoDatabase,
        CancellationToken cancellationToken = default)
    {
        IAsyncCursor<string> cursor = await mongoDatabase
            .ListCollectionNamesAsync(cancellationToken: cancellationToken);

        foreach (var name in await cursor.ToListAsync(cancellationToken))
        {
            IMongoCollection<BsonDocument> collection =
                mongoDatabase.GetCollection<BsonDocument>(name);

            await collection.CleanCollectionAsync(cancellationToken);
        }
    }

    /// <summary>
    /// Dumps all collections and returns it in a Dictionary.
    /// </summary>
    /// <param name="mongoDatabase">The database to dump all collections.</param>
    public static IEnumerable<KeyValuePair<string, IEnumerable<BsonDocument>>> DumpAllCollections(
        this IMongoDatabase mongoDatabase)
    {
        List<KeyValuePair<string, IEnumerable<BsonDocument>>> dumpedCollections =
            new List<KeyValuePair<string, IEnumerable<BsonDocument>>>();

        foreach (var name in mongoDatabase.ListCollectionNames().ToList())
        {
            IEnumerable<BsonDocument> dumpedCollection =
                mongoDatabase.GetCollection<BsonDocument>(name).Dump();

            dumpedCollections.Add(
                new KeyValuePair<string, IEnumerable<BsonDocument>>(
                    name, dumpedCollection));
        }

        return dumpedCollections;
    }
}
