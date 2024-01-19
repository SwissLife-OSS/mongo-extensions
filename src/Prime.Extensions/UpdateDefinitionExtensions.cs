using MongoDB.Bson.IO;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Bson.Serialization;

namespace MongoDB.Prime.Extensions
{
    public static class UpdateDefinitionExtensions
    {
        public static string ToDefinitionString<T>(
            this UpdateDefinition<T> filter, bool indent = false)
        {
            BsonValue bson = ToBsonValue(filter);

            var settings = new JsonWriterSettings
            {
                Indent = indent,
                OutputMode = JsonOutputMode.Shell,
            };

            string json = bson.ToJson(writerSettings: settings);

            return json;
        }

        public static BsonValue ToBsonValue<T>(
            this UpdateDefinition<T> filter)
        {
            IBsonSerializerRegistry serializerRegistry =
                BsonSerializer.SerializerRegistry;

            return filter.Render(serializerRegistry
                .GetSerializer<T>(), serializerRegistry);
        }
    }
}
