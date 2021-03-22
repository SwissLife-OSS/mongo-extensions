using System.Text.Json;
using MongoDB.Bson;
using MongoDB.Bson.IO;

namespace MongoDB.Extensions.Context
{
    public static class BsonDocumentExtensions
    {
        public static JsonDocument ToJsonDocument(this BsonDocument bsonDocument)
        {
            string json = bsonDocument.ToJson(
                new JsonWriterSettings { OutputMode = JsonOutputMode.RelaxedExtendedJson });

            return JsonDocument.Parse(json);
        }
    }
}
