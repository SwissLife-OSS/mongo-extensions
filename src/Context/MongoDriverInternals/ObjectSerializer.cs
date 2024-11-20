using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;

namespace MongoDB.Extensions.Context;

/* Copyright 2010-present MongoDB Inc.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
public static class DefaultFrameworkAllowedTypes
{
    private readonly static Func<Type, bool> __allowedTypes = AllowedTypesImplementation;

    private readonly static HashSet<Type> __allowedNonGenericTypesSet = new HashSet<Type>
    {
        typeof(bool),
        typeof(byte),
        typeof(char),
        typeof(System.Collections.ArrayList),
        typeof(System.Collections.BitArray),
        typeof(System.Collections.Hashtable),
        typeof(System.Collections.Queue),
        typeof(System.Collections.SortedList),
        typeof(System.Collections.Specialized.ListDictionary),
        typeof(System.Collections.Specialized.OrderedDictionary),
        typeof(System.Collections.Stack),
        typeof(System.DateTime),
        typeof(System.DateTimeOffset),
        typeof(decimal),
        typeof(double),
        typeof(System.Dynamic.ExpandoObject),
        typeof(System.Guid),
        typeof(short),
        typeof(int),
        typeof(long),
        typeof(System.Net.DnsEndPoint),
        typeof(System.Net.EndPoint),
        typeof(System.Net.IPAddress),
        typeof(System.Net.IPEndPoint),
        typeof(System.Net.IPHostEntry),
        typeof(object),
        typeof(sbyte),
        typeof(float),
        typeof(string),
        typeof(System.Text.RegularExpressions.Regex),
        typeof(System.TimeSpan),
        typeof(ushort),
        typeof(uint),
        typeof(ulong),
        typeof(System.Uri),
        typeof(System.Version)
    };

    private readonly static HashSet<Type> __allowedGenericTypesSet = new HashSet<Type>
    {
        typeof(System.Collections.Generic.Dictionary<,>),
        typeof(System.Collections.Generic.HashSet<>),
        typeof(System.Collections.Generic.KeyValuePair<,>),
        typeof(System.Collections.Generic.LinkedList<>),
        typeof(System.Collections.Generic.List<>),
        typeof(System.Collections.Generic.Queue<>),
        typeof(System.Collections.Generic.SortedDictionary<,>),
        typeof(System.Collections.Generic.SortedList<,>),
        typeof(System.Collections.Generic.SortedSet<>),
        typeof(System.Collections.Generic.Stack<>),
        typeof(System.Collections.ObjectModel.Collection<>),
        typeof(System.Collections.ObjectModel.KeyedCollection<,>),
        typeof(System.Collections.ObjectModel.ObservableCollection<>),
        typeof(System.Collections.ObjectModel.ReadOnlyCollection<>),
        typeof(System.Collections.ObjectModel.ReadOnlyDictionary<,>),
        typeof(System.Collections.ObjectModel.ReadOnlyObservableCollection<>),
        typeof(System.Nullable<>),
        typeof(System.Tuple<>),
        typeof(System.Tuple<,>),
        typeof(System.Tuple<,,>),
        typeof(System.Tuple<,,,>),
        typeof(System.Tuple<,,,,>),
        typeof(System.Tuple<,,,,,>),
        typeof(System.Tuple<,,,,,,>),
        typeof(System.Tuple<,,,,,,,>),
        typeof(System.ValueTuple<,,,,,,,>),
        typeof(System.ValueTuple<>),
        typeof(System.ValueTuple<,>),
        typeof(System.ValueTuple<,,>),
        typeof(System.ValueTuple<,,,>),
        typeof(System.ValueTuple<,,,,>),
        typeof(System.ValueTuple<,,,,,>),
        typeof(System.ValueTuple<,,,,,,>),
        typeof(System.ValueTuple<,,,,,,,>)
    };

    public static Func<Type, bool> AllowedTypes => __allowedTypes;

    private static bool AllowedTypesImplementation(Type type)
    {
        return type.IsConstructedGenericType ? IsAllowedGenericType(type) : IsAllowedType(type);

        static bool IsAllowedType(Type type) =>
            typeof(BsonValue).IsAssignableFrom(type) ||
            __allowedNonGenericTypesSet.Contains(type) ||
            type.IsArray && AllowedTypesImplementation(type.GetElementType()) ||
            type.IsEnum;

        static bool IsAllowedGenericType(Type type) =>
            (__allowedGenericTypesSet.Contains(type.GetGenericTypeDefinition()) || type.IsAnonymousType()) &&
            type.GetGenericArguments().All(__allowedTypes);
    }
}
