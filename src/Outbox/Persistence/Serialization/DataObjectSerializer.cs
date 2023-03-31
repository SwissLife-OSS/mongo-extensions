using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Bson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using MongoDB.Extensions.Outbox.Core;

namespace MongoDB.Extensions.Outbox.Persistence.Serialization
{
    /// <summary>
    /// From https://github.com/danielgerlag/workflow-core/
    /// Thanks @Daniel Gerlag
    /// </summary>
    public class DataObjectSerializer : SerializerBase<object>
    {
        private static JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Objects,
        };

        public override object Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            if (context.Reader.CurrentBsonType == BsonType.String)
            {
                var raw = BsonSerializer.Deserialize<string>(context.Reader);
                return Newtonsoft.Json.JsonConvert.DeserializeObject<object>(raw, SerializerSettings);
            }

            object? obj = BsonSerializer.Deserialize(context.Reader, typeof(object));
            return obj;
        }

        public override void Serialize(
            BsonSerializationContext context,
            BsonSerializationArgs args,
            object value)
        {
            BsonDocument doc;
            if (BsonClassMap.IsClassMapRegistered(value.GetType()))
            {
                doc = value.ToBsonDocument();
                doc.Remove("_t");
                doc.InsertAt(0, new BsonElement("_t", value.GetType().AssemblyQualifiedName));
                AddTypeInformation(doc.Elements, value, string.Empty);
            }
            else
            {
                var str = Newtonsoft.Json.JsonConvert.SerializeObject(value, SerializerSettings);
                doc = BsonDocument.Parse(str);
                ConvertMetaFormat(doc);
            }

            BsonSerializer.Serialize(context.Writer, doc);
        }

        private void AddTypeInformation(IEnumerable<BsonElement> elements, object value, string xPath)
        {
            foreach (BsonElement element in elements)
            {
                var elementXPath = string.IsNullOrEmpty(xPath) ? element.Name : xPath + "." + element.Name;
                if (element.Value.IsBsonDocument)
                {
                    BsonDocument doc = element.Value.AsBsonDocument;
                    doc.Remove("_t");
                    doc.InsertAt(0, new BsonElement("_t", GetTypeNameFromXPath(value, elementXPath)));
                    AddTypeInformation(doc.Elements, value, elementXPath);
                }
                if (element.Value.IsBsonArray)
                {
                    AddTypeInformation(element.Value.AsBsonArray, value, elementXPath);
                }
            }
        }

        private string GetTypeNameFromXPath(object root, string xPath)
        {
            var parts = xPath.Split('.').ToList();
            var value = root;
            while (parts.Count > 0)
            {
                var subPath = parts[0];
                if (subPath[0] == '[')
                {
                    var index = int.Parse(subPath.Trim('[', ']'));
                    if (value is IList || value.GetType().IsArray)
                    {
                        var list = (IList)value;
                        value = list[index];
                    }
                    else
                    {
                        throw new NotSupportedException();
                    }
                }
                else
                {
                    System.Reflection.PropertyInfo? propInfo
                        = value.GetType().GetProperty(subPath);
                    value = propInfo.GetValue(value);
                }

                parts.RemoveAt(0);
            }

            return value.GetType().AssemblyQualifiedName;
        }

        private void AddTypeInformation(IEnumerable<BsonValue> elements, object value, string xPath)
        {
            //foreach (var element in elements)
            for (var i = 0; i < elements.Count(); i++)
            {
                BsonValue element = elements.ElementAt(i);
                if (element.IsBsonDocument)
                {
                    BsonDocument doc = element.AsBsonDocument;
                    var elementXPath = xPath + $".[{i}]";
                    doc.Remove("_t");
                    doc.InsertAt(0, new BsonElement("_t", GetTypeNameFromXPath(value, elementXPath)));
                    AddTypeInformation(doc.Elements, value, elementXPath);
                }

                if (element.IsBsonArray)
                {
                    AddTypeInformation(element.AsBsonArray, value, xPath);
                }
            }
        }

        private static void ConvertMetaFormat(BsonDocument root)
        {
            var stack = new Stack<BsonDocument>();
            stack.Push(root);

            while (stack.Count > 0)
            {
                BsonDocument doc = stack.Pop();

                if (doc.TryGetElement("$type", out BsonElement typeElem))
                {
                    doc.RemoveElement(typeElem);

                    if (doc.Elements.All(x => x.Name != "_t"))
                        doc.InsertAt(0, new BsonElement("_t", typeElem.Value));
                }

                foreach (BsonElement subDoc in doc.Elements)
                {
                    if (subDoc.Value.IsBsonDocument)
                        stack.Push(subDoc.Value.ToBsonDocument());

                    if (subDoc.Value.IsBsonArray)
                    {
                        foreach (BsonValue? element in subDoc.Value.AsBsonArray)
                        {
                            if (element.IsBsonDocument)
                                stack.Push(element.ToBsonDocument());
                        }
                    }
                }
            }
        }
    }
}
