using System;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace MongoDB.Extensions.Migration;

class MigrationSerializer<T> : IBsonSerializer<T> where T : IVersioned
{
    private readonly EntityContext _context;
    private readonly MigrationRunner<T> _migrationRunner;
    private readonly IBsonSerializer<T> _baseSerializer;

    public MigrationSerializer(EntityContext context)
    {
        _context = context;
        _migrationRunner = new MigrationRunner<T>(context);
        _baseSerializer = BsonSerializer.LookupSerializer<T>();
    }

    object IBsonSerializer.Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        return Deserialize(context, args);
    }

    public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, object value)
    {
        if (value is T typedValue)
        {
            typedValue.Version = _context.Option.CurrentVersion;
            _baseSerializer.Serialize(context, args, typedValue);
        }
        else
        {
            throw new ArgumentException($"Expected value to be of type {typeof(T)}, but was {value.GetType()}.", nameof(value));
        }
    }

    public Type ValueType => typeof(T);

    public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, T value)
    {
        value.Version = _context.Option.CurrentVersion;
        _baseSerializer.Serialize(context, args, value);
    }

    public T Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        BsonDocument bsonDocument = BsonDocumentSerializer.Instance.Deserialize(context);

        _migrationRunner.Run(bsonDocument);

        var migratedContext = BsonDeserializationContext.CreateRoot(new BsonDocumentReader(bsonDocument));

        T entity = _baseSerializer.Deserialize(migratedContext, args);

        entity.Version = _context.Option.CurrentVersion;

        return entity;
    }
}
