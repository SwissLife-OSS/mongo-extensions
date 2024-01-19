using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace MongoDB.Extensions.Migration;

class MigrationSerializer<T> : BsonClassMapSerializer<T> where T : IVersioned
{
    private readonly EntityContext _context;
    private readonly MigrationRunner<T> _migrationRunner;

    public MigrationSerializer(EntityContext context) : base(BsonClassMap.LookupClassMap(typeof(T)))
    {
        _context = context;
        _migrationRunner = new MigrationRunner<T>(context);
    }


    public override void Serialize(
        BsonSerializationContext context,
        BsonSerializationArgs args,
        T value)
    {
        value.Version = _context.Option.CurrentVersion;
        base.Serialize(context, args, value);
    }

    public override T Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        BsonDocument bsonDocument = BsonDocumentSerializer.Instance.Deserialize(context);

        _migrationRunner.Run(bsonDocument);

        var migratedContext =
            BsonDeserializationContext.CreateRoot(new BsonDocumentReader(bsonDocument));

        T entity = base.Deserialize(migratedContext, args);

        entity.Version = _context.Option.CurrentVersion;

        return entity;
    }
}
