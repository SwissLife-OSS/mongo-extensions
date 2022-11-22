using MongoDB.Bson.IO;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Bson.Serialization;

namespace MongoDB.Prime.Extensions
{
    public static class FilterDefinitionExtensions
    {
        public static string ToDefinitionString<T>(
            this FilterDefinition<T> filter, bool indent = false)
        {
            BsonDocument bson = ToBsonDocument(filter);

            var settings = new JsonWriterSettings
            {
                Indent = indent,
                OutputMode = JsonOutputMode.Shell,
            };

            string json = bson.ToJson(writerSettings: settings);

            return json;
        }

        public static BsonDocument ToBsonDocument<T>(
            this FilterDefinition<T> filter)
        {
            IBsonSerializerRegistry serializerRegistry =
                BsonSerializer.SerializerRegistry;

            return filter.Render(serializerRegistry
                .GetSerializer<T>(), serializerRegistry);
        }
    }
}
