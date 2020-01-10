using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;

namespace MongoDB.Extensions.Context
{
    public class ImmutablePocoConvention
        : ConventionBase
        , IClassMapConvention
    {
        private readonly BindingFlags _bindingFlags;

        public ImmutablePocoConvention()
            : this(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
        {
        }

        public ImmutablePocoConvention(BindingFlags bindingFlags)
        {
            _bindingFlags = bindingFlags;
        }

        public void Apply(BsonClassMap classMap)
        {
            var properties = classMap.ClassType
                .GetTypeInfo()
                .GetProperties(_bindingFlags)
                .ToList();

            var mappingProperties = properties
                .Where(p => IsReadOnlyProperty(classMap, p))
                .ToList();

            foreach (PropertyInfo property in mappingProperties)
            {
                classMap.MapMember(property);
            }

            if (!classMap.ClassType.IsAbstract)
            {
                foreach (ConstructorInfo constructor in classMap.ClassType.GetConstructors(
                    BindingFlags.Instance |
                    BindingFlags.NonPublic |
                    BindingFlags.Public))
                {
                    List<PropertyInfo> matchProperties =
                        GetMatchingProperties(constructor, properties);

                    if (matchProperties.Any())
                    {
                        BsonCreatorMap creatorMap = classMap.MapConstructor(constructor);
                        creatorMap.SetArguments(matchProperties);
                    }
                }
            }
        }

        private static List<PropertyInfo> GetMatchingProperties(
            ConstructorInfo constructor,
            List<PropertyInfo> properties)
        {
            var matchProperties = new List<PropertyInfo>();

            ParameterInfo[] ctorParameters = constructor.GetParameters();
            foreach (ParameterInfo ctorParameter in ctorParameters)
            {
                PropertyInfo matchProperty = properties
                    .FirstOrDefault(p => ParameterMatchProperty(ctorParameter, p));

                if (matchProperty == null)
                {
                    return new List<PropertyInfo>();
                }

                matchProperties.Add(matchProperty);
            }

            return matchProperties;
        }


        private static bool ParameterMatchProperty(
            ParameterInfo parameter,
            PropertyInfo property)
        {
            return string.Equals(
                       property.Name,
                       parameter.Name,
                       System.StringComparison.InvariantCultureIgnoreCase) &&
                   parameter.ParameterType == property.PropertyType;
        }

        private static bool IsReadOnlyProperty(
            BsonClassMap classMap,
            PropertyInfo propertyInfo)
        {
            if (!propertyInfo.CanRead)
            {
                return false;
            }

            if (propertyInfo.CanWrite)
            {
                return false;
            }

            if (propertyInfo.GetIndexParameters().Length != 0)
            {
                return false;
            }

            MethodInfo getMethodInfo = propertyInfo.GetMethod;

            if (getMethodInfo.IsAbstract ||
                IsBaseTypeProperty(classMap, getMethodInfo) &&
                !IsOverrideProperty(classMap, getMethodInfo))
            {
                return false;
            }

            return true;
        }

        private static bool IsBaseTypeProperty(BsonClassMap classMap, MethodInfo getMethodInfo)
        {
            return getMethodInfo.GetBaseDefinition().DeclaringType != classMap.ClassType;
        }

        private static bool IsOverrideProperty(BsonClassMap classMap, MethodInfo getMethodInfo)
        {
            return getMethodInfo.DeclaringType == classMap.ClassType;
        }
    }
}
