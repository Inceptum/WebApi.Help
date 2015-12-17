using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Inceptum.WebApi.Help.Common;

namespace Inceptum.WebApi.Help.SampleGeneration
{
    /// <summary>
    /// This class creates an object of a given type and populates it with sample data.
    /// </summary>
    internal class ObjectGenerator
    {
        private const int DEFAULT_COLLECTION_SIZE = 2;

        private readonly IDictionary<Type, ValueHolder> m_SampleObjects;
        private readonly SimpleTypeObjectGenerator m_SimpleObjectGenerator = new SimpleTypeObjectGenerator();

        public ObjectGenerator(IDictionary<Type, ValueHolder> sampleObjects = null)
        {
            m_SampleObjects = sampleObjects ?? new Dictionary<Type, ValueHolder>();
        }

        /// <summary>
        /// Generates an object for a given type. The type needs to be public, have a public default constructor and settable
        /// public properties/fields. Currently it supports the following types:
        /// Simple types: <see cref="int" />, <see cref="string" />, <see cref="Enum" />, <see cref="DateTime" />, <see cref="Uri" />, etc.
        /// Complex types: POCO types.
        /// Nullables: <see cref="Nullable{T}" />.
        /// Arrays: arrays of simple types or complex types.
        /// Key value pairs: <see cref="KeyValuePair{TKey,TValue}" />
        /// Tuples: <see cref="Tuple{T1}" />, <see cref="Tuple{T1,T2}" />, etc
        /// Dictionaries: <see cref="IDictionary{TKey,TValue}" /> or anything deriving from <see cref="IDictionary{TKey,TValue}" />.
        /// Collections: <see cref="IList{T}" />, <see cref="IEnumerable{T}" />, <see cref="ICollection{T}" />, <see cref="IList" />, <see cref="IEnumerable" />, <see cref="ICollection" /> 
        ///     or anything deriving from <see cref="ICollection{T}" /> or <see cref="IList" />.
        /// Queryables: <see cref="IQueryable" />, <see cref="IQueryable{T}" />.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>An object of the given type.</returns>
        public object GenerateObject(Type type)
        {
            return generateObject(type, new Dictionary<Type, object>());
        }

        private ObjectGenerator createNestedGenerator()
        {
            return new ObjectGenerator(m_SampleObjects);
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Here we just want to return null if anything goes wrong.")]
        private object generateObject(Type type, Dictionary<Type, object> createdObjectReferences)
        {
            try
            {
                ValueHolder sample;
                if (m_SampleObjects.TryGetValue(type, out sample))
                {
                    return sample.Value;
                }

                if (SimpleTypeObjectGenerator.CanGenerateObject(type))
                {
                    return m_SimpleObjectGenerator.GenerateObject(type);
                }

                if (type.IsArray)
                {
                    return generateArray(type, DEFAULT_COLLECTION_SIZE, createdObjectReferences);
                }

                if (type.IsGenericType)
                {
                    return generateGenericType(type, DEFAULT_COLLECTION_SIZE, createdObjectReferences);
                }

                if (type == typeof(IDictionary))
                {
                    return generateDictionary(typeof(Hashtable), DEFAULT_COLLECTION_SIZE, createdObjectReferences);
                }

                if (typeof(IDictionary).IsAssignableFrom(type))
                {
                    return generateDictionary(type, DEFAULT_COLLECTION_SIZE, createdObjectReferences);
                }

                if (type == typeof(IList) ||
                    type == typeof(IEnumerable) ||
                    type == typeof(ICollection))
                {
                    return generateCollection(typeof(ArrayList), DEFAULT_COLLECTION_SIZE, createdObjectReferences);
                }

                if (typeof(IList).IsAssignableFrom(type))
                {
                    return generateCollection(type, DEFAULT_COLLECTION_SIZE, createdObjectReferences);
                }

                if (type == typeof(IQueryable))
                {
                    return generateQueryable(type, DEFAULT_COLLECTION_SIZE, createdObjectReferences);
                }

                if (type.IsEnum)
                {
                    return generateEnum(type);
                }

                if (type.IsPublic || type.IsNestedPublic)
                {
                    return generateComplexObject(type, createdObjectReferences);
                }
            }
            catch
            {
                // Returns null if anything fails
                return null;
            }

            return null;
        }

        private object generateGenericType(Type type, int collectionSize, Dictionary<Type, object> createdObjectReferences)
        {
            Type genericTypeDefinition = type.GetGenericTypeDefinition();
            if (genericTypeDefinition == typeof(Nullable<>))
            {
                return generateNullable(type, createdObjectReferences);
            }

            if (genericTypeDefinition == typeof(KeyValuePair<,>))
            {
                return generateKeyValuePair(type, createdObjectReferences);
            }

            if (isTuple(genericTypeDefinition))
            {
                return generateTuple(type, createdObjectReferences);
            }

            Type[] genericArguments = type.GetGenericArguments();
            if (genericArguments.Length == 1)
            {
                if (genericTypeDefinition == typeof(IList<>) ||
                    genericTypeDefinition == typeof(IEnumerable<>) ||
                    genericTypeDefinition == typeof(ICollection<>))
                {
                    Type collectionType = typeof(List<>).MakeGenericType(genericArguments);
                    return generateCollection(collectionType, collectionSize, createdObjectReferences);
                }

                if (genericTypeDefinition == typeof(IQueryable<>))
                {
                    return generateQueryable(type, collectionSize, createdObjectReferences);
                }

                Type closedCollectionType = typeof(ICollection<>).MakeGenericType(genericArguments[0]);
                if (closedCollectionType.IsAssignableFrom(type))
                {
                    return generateCollection(type, collectionSize, createdObjectReferences);
                }
            }

            if (genericArguments.Length == 2)
            {
                if (genericTypeDefinition == typeof(IDictionary<,>))
                {
                    Type dictionaryType = typeof(Dictionary<,>).MakeGenericType(genericArguments);
                    return generateDictionary(dictionaryType, collectionSize, createdObjectReferences);
                }

                Type closedDictionaryType = typeof(IDictionary<,>).MakeGenericType(genericArguments[0],
                    genericArguments[1]);
                if (closedDictionaryType.IsAssignableFrom(type))
                {
                    return generateDictionary(type, collectionSize, createdObjectReferences);
                }
            }

            if (type.IsPublic || type.IsNestedPublic)
            {
                return generateComplexObject(type, createdObjectReferences);
            }

            return null;
        }

        private object generateTuple(Type type, Dictionary<Type, object> createdObjectReferences)
        {
            Type[] genericArgs = type.GetGenericArguments();
            var parameterValues = new object[genericArgs.Length];
            bool failedToCreateTuple = true;
            var objectGenerator = createNestedGenerator();
            for (int i = 0; i < genericArgs.Length; i++)
            {
                parameterValues[i] = objectGenerator.generateObject(genericArgs[i], createdObjectReferences);
                failedToCreateTuple &= parameterValues[i] == null;
            }
            if (failedToCreateTuple)
            {
                return null;
            }
            object result = Activator.CreateInstance(type, parameterValues);
            return result;
        }

        private static bool isTuple(Type genericTypeDefinition)
        {
            return genericTypeDefinition == typeof(Tuple<>) ||
                   genericTypeDefinition == typeof(Tuple<,>) ||
                   genericTypeDefinition == typeof(Tuple<,,>) ||
                   genericTypeDefinition == typeof(Tuple<,,,>) ||
                   genericTypeDefinition == typeof(Tuple<,,,,>) ||
                   genericTypeDefinition == typeof(Tuple<,,,,,>) ||
                   genericTypeDefinition == typeof(Tuple<,,,,,,>) ||
                   genericTypeDefinition == typeof(Tuple<,,,,,,,>);
        }

        private object generateKeyValuePair(Type keyValuePairType, Dictionary<Type, object> createdObjectReferences)
        {
            Type[] genericArgs = keyValuePairType.GetGenericArguments();
            Type typeK = genericArgs[0];
            Type typeV = genericArgs[1];
            var objectGenerator = createNestedGenerator();
            object keyObject = objectGenerator.generateObject(typeK, createdObjectReferences);
            object valueObject = objectGenerator.generateObject(typeV, createdObjectReferences);
            if (keyObject == null && valueObject == null)
            {
                // Failed to create key and values
                return null;
            }
            object result = Activator.CreateInstance(keyValuePairType, keyObject, valueObject);
            return result;
        }

        private object generateArray(Type arrayType, int size, Dictionary<Type, object> createdObjectReferences)
        {
            Type type = arrayType.GetElementType();
            Array result = Array.CreateInstance(type, size);
            bool areAllElementsNull = true;
            var objectGenerator = createNestedGenerator();
            for (int i = 0; i < size; i++)
            {
                object element = objectGenerator.generateObject(type, createdObjectReferences);
                result.SetValue(element, i);
                areAllElementsNull &= element == null;
            }

            if (areAllElementsNull)
            {
                return null;
            }

            return result;
        }

        private object generateDictionary(Type dictionaryType, int size, Dictionary<Type, object> createdObjectReferences)
        {
            Type typeK = typeof(object);
            Type typeV = typeof(object);
            if (dictionaryType.IsGenericType)
            {
                Type[] genericArgs = dictionaryType.GetGenericArguments();
                typeK = genericArgs[0];
                typeV = genericArgs[1];
            }

            object result = Activator.CreateInstance(dictionaryType);
            MethodInfo addMethod = dictionaryType.GetMethod("Add") ?? dictionaryType.GetMethod("TryAdd");
            MethodInfo containsMethod = dictionaryType.GetMethod("Contains") ?? dictionaryType.GetMethod("ContainsKey");
            var objectGenerator = createNestedGenerator();
            for (int i = 0; i < size; i++)
            {
                object newKey = objectGenerator.generateObject(typeK, createdObjectReferences);
                if (newKey == null)
                {
                    // Cannot generate a valid key
                    return null;
                }

                var containsKey = (bool)containsMethod.Invoke(result, new[] { newKey });
                if (!containsKey)
                {
                    object newValue = objectGenerator.generateObject(typeV, createdObjectReferences);
                    addMethod.Invoke(result, new[] { newKey, newValue });
                }
            }

            return result;
        }

        private static object generateEnum(Type enumType)
        {
            Array possibleValues = Enum.GetValues(enumType);
            if (possibleValues.Length > 0)
            {
                return possibleValues.GetValue(0);
            }
            return null;
        }

        private object generateQueryable(Type queryableType, int size, Dictionary<Type, object> createdObjectReferences)
        {
            bool isGeneric = queryableType.IsGenericType;
            object list;
            if (isGeneric)
            {
                Type listType = typeof(List<>).MakeGenericType(queryableType.GetGenericArguments());
                list = generateCollection(listType, size, createdObjectReferences);
            }
            else
            {
                list = generateArray(typeof(object[]), size, createdObjectReferences);
            }
            if (list == null)
            {
                return null;
            }
            if (isGeneric)
            {
                Type argumentType = typeof(IEnumerable<>).MakeGenericType(queryableType.GetGenericArguments());
                MethodInfo asQueryableMethod = typeof(Queryable).GetMethod("AsQueryable", new[] { argumentType });
                return asQueryableMethod.Invoke(null, new[] { list });
            }

            return ((IEnumerable)list).AsQueryable();
        }

        private object generateCollection(Type collectionType, int size, Dictionary<Type, object> createdObjectReferences)
        {
            Type type = collectionType.IsGenericType
                ? collectionType.GetGenericArguments()[0]
                : typeof(object);
            object result = Activator.CreateInstance(collectionType);
            MethodInfo addMethod = collectionType.GetMethod("Add");
            bool areAllElementsNull = true;
            var objectGenerator = createNestedGenerator();
            for (int i = 0; i < size; i++)
            {
                object element = objectGenerator.generateObject(type, createdObjectReferences);
                addMethod.Invoke(result, new[] { element });
                areAllElementsNull &= element == null;
            }

            if (areAllElementsNull)
            {
                return null;
            }

            return result;
        }

        private object generateNullable(Type nullableType, Dictionary<Type, object> createdObjectReferences)
        {
            Type type = nullableType.GetGenericArguments()[0];
            var objectGenerator = createNestedGenerator();
            return objectGenerator.generateObject(type, createdObjectReferences);
        }

        private object generateComplexObject(Type type, Dictionary<Type, object> createdObjectReferences)
        {
            object result;
            if (createdObjectReferences.TryGetValue(type, out result))
            {
                // The object has been created already, just return it. This will handle the circular reference case.
                return result;
            }

            if (type.IsValueType)
            {
                result = Activator.CreateInstance(type);
            }
            else
            {
                ConstructorInfo defaultCtor = type.GetConstructor(Type.EmptyTypes);
                if (defaultCtor == null)
                {
                    // Cannot instantiate the type because it doesn't have a default constructor
                    return null;
                }

                result = defaultCtor.Invoke(new object[0]);
            }
            createdObjectReferences.Add(type, result);
            setPublicProperties(type, result, createdObjectReferences);
            setPublicFields(type, result, createdObjectReferences);
            return result;
        }

        private void setPublicProperties(Type type, object obj, Dictionary<Type, object> createdObjectReferences)
        {
            PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var objectGenerator = createNestedGenerator();
            foreach (PropertyInfo property in properties)
            {
                if (property.CanWrite)
                {
                    object propertyValue = objectGenerator.generateObject(property.PropertyType, createdObjectReferences);
                    property.SetValue(obj, propertyValue, null);
                }
            }
        }

        private void setPublicFields(Type type, object obj, Dictionary<Type, object> createdObjectReferences)
        {
            FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
            var objectGenerator = createNestedGenerator();
            foreach (FieldInfo field in fields)
            {
                object fieldValue = objectGenerator.generateObject(field.FieldType, createdObjectReferences);
                field.SetValue(obj, fieldValue);
            }
        }

        private sealed class SimpleTypeObjectGenerator
        {
            private static readonly Dictionary<Type, Func<long, object>> m_DefaultGenerators = initializeGenerators();
            private long m_Index;

            [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "These are simple type factories and cannot be split up.")]
            private static Dictionary<Type, Func<long, object>> initializeGenerators()
            {
                return new Dictionary<Type, Func<long, object>>
                {
                    {typeof (Boolean), index => true},
                    {typeof (Byte), index => (Byte) 64},
                    {typeof (Char), index => (Char) 65},
                    {typeof (DateTime), index => DateTime.Now},
                    {typeof (DateTimeOffset), index => new DateTimeOffset(DateTime.Now)},
                    {typeof (DBNull), index => DBNull.Value},
                    {typeof (Decimal), index => (Decimal) index},
                    {typeof (Double), index => index + 0.1},
                    {typeof (Guid), index => Guid.NewGuid()},
                    {typeof (Int16), index => (Int16) (index%Int16.MaxValue)},
                    {typeof (Int32), index => (Int32) (index%Int32.MaxValue)},
                    {typeof (Int64), index => index},
                    {typeof (Object), index => new object()},
                    {typeof (SByte), index => (SByte) 64},
                    {typeof (Single), index => (Single) (index + 0.1)},
                    { typeof (String), index => String.Format(CultureInfo.CurrentCulture, "sample string {0}", index) },
                    {typeof (TimeSpan), index => TimeSpan.FromTicks(1234567) },
                    {typeof (UInt16), index => (UInt16) (index%UInt16.MaxValue)},
                    {typeof (UInt32), index => (UInt32) (index%UInt32.MaxValue)},
                    {typeof (UInt64), index => (UInt64) index},
                    {typeof (Uri), index => new Uri(String.Format(CultureInfo.CurrentCulture, "http://webapihelppage{0}.com", index)) },
                };
            }

            public static bool CanGenerateObject(Type type)
            {
                return m_DefaultGenerators.ContainsKey(type);
            }

            public object GenerateObject(Type type)
            {
                return m_DefaultGenerators[type](++m_Index);
            }
        }
    }
}