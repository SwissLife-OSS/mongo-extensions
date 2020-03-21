using MongoDB.Bson;
using MongoDB.Driver;

namespace MongoDB.Extensions.Context
{
    public static class MongoServerExtensions
    {
        public static void DisableTableScan(this IMongoClient mongoClient)
        {
            var command = new BsonDocument { { "setParameter", 1 }, { "notablescan", 1 } };

            mongoClient.GetDatabase("admin").RunCommand<BsonDocument>(command);
        }
        
        public static bool IsTableScanDisabled(this IMongoClient mongoClient)
        {
            var getNoTableScanCommand =
                new BsonDocument { { "getParameter", 1 }, { "notablescan", 1 } };

            BsonDocument parameterResult = mongoClient.GetDatabase("admin")
                .RunCommand<BsonDocument>(getNoTableScanCommand);

            BsonElement notablescanElement = parameterResult.GetElement("notablescan");
            
            if(bool.TryParse(notablescanElement.Value.ToString(), out bool isDisabled))
            {
                return isDisabled;
            }

            return false;
        }
    }
}
