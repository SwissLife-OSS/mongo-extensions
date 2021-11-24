using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;

namespace MongoDB.Extensions.Context
{
    internal class ImmutableConvention
        : ConventionBase
        , IClassMapConvention
    {
        private readonly BindingFlags _bindingFlags;
        private readonly string _externalInitTypeName =
            "System.Runtime.CompilerServices.IsExternalInit";
        private readonly string _nullableAttributeFullName =
            "System.Runtime.CompilerServices.NullableAttribute";

        public ImmutableConvention()
            : this(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
        {
        }

        public ImmutableConvention(BindingFlags bindingFlags)
        {
            _bindingFlags = bindingFlags;
        }

        public void Apply(BsonClassMap classMap)
        {
            var properties = classMap.ClassType
                .GetTypeInfo()
                .GetProperties(_bindingFlags)
                .Where(p => p.PropertyType != typeof(Type))
                .ToList();

            var mappingProperties = properties
                .Where(p => IsReadOnlyProperty(classMap, p) || IsInitOnlyProperty(p))
                .ToList();

            foreach (PropertyInfo property in mappingProperties)
            {
                BsonMemberMap member = classMap.MapMember(property);
                if (IsNullableProperty(property))
                {
                    member.SetDefaultValue((object?)null);
                }
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

        private bool IsNullableProperty(PropertyInfo propertyInfo)
        {
            return Nullable.GetUnderlyingType(propertyInfo.PropertyType) != null ||
                   propertyInfo.CustomAttributes.Any(a =>
                       a.AttributeType.FullName == _nullableAttributeFullName);
        }

        private static List<PropertyInfo> GetMatchingProperties(
            ConstructorInfo constructor,
            List<PropertyInfo> properties)
        {
            var matchProperties = new List<PropertyInfo>();

            ParameterInfo[] ctorParameters = constructor.GetParameters();
            foreach (ParameterInfo ctorParameter in ctorParameters)
            {
                PropertyInfo? matchProperty = properties
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
                       StringComparison.InvariantCultureIgnoreCase) &&
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

        private bool IsInitOnlyProperty(PropertyInfo property)
        {
            if (!property.CanWrite)
            {
                return false;
            }

            var setModifiers = property.SetMethod?.ReturnParameter?.GetRequiredCustomModifiers();
            var containsInit = setModifiers?.Any(m =>
                m.FullName == _externalInitTypeName);
            return containsInit ?? false;
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
