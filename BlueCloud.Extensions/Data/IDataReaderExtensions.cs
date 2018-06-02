using System;
using System.Data;
using System.Collections.Generic;
using System.Reflection;

namespace BlueCloud.Extensions.Data
{
    public static class IDataReaderExtensions
    {
        /// <summary>
        /// Returns a value from the database with the given type.
        /// </summary>
        /// <typeparam name="T">Data Type</typeparam>
        /// <param name="fieldName">Database Field Name</param>
        /// <returns></returns>
        public static T GetValue<T>(this IDataReader dataReader, string fieldName)
        {
            if (fieldName == null)
                throw new ArgumentNullException("fieldName");

            object value = null;

            try
            {
                int ordinal = dataReader.GetOrdinal(fieldName);
                Type type = typeof(T);

                // Check for Nullables
                Type underlyingType = Nullable.GetUnderlyingType(type);
                bool isNullableType = underlyingType != null;
                bool isValueType = type.IsValueType;

                // Handle Null Values
                if (dataReader.IsDBNull(ordinal))
                {
                    if (isValueType && !isNullableType)
                    {
                        throw new Exception($"Attempting to assign null to a non-nullable type for field: '{fieldName}'");
                    }

                    return default(T);
                }

                value = dataReader.GetValue(ordinal);

                T result = isNullableType ? (T)Convert.ChangeType(value, underlyingType) : (T)Convert.ChangeType(value, type);

                if (type == typeof(DateTime))
                {
                    var date = (DateTime)Convert.ChangeType(result, typeof(DateTime));
                    result = (T)Convert.ChangeType(DateTime.SpecifyKind(date, DateTimeKind.Local), typeof(T));
                }

                return result;
            }
            catch (IndexOutOfRangeException ex)
            {
                throw new IndexOutOfRangeException($"Fieldname {fieldName} does not exist in query", ex);
            }
            catch (InvalidCastException castEx)
            {
                if (value != null)
                {
                    throw new InvalidCastException($"Invalid cast. Expected type '{value.GetType().Name}' for field: {fieldName}", castEx);
                }
                else
                {
                    throw;
                }
            }
        }

        private static readonly Dictionary<IDataRecord, Dictionary<string, int>> memo = new Dictionary<IDataRecord, Dictionary<string, int>>();

        public static Dictionary<string, int> GetColumnOrdinals(this IDataRecord record)
        {
            if (memo.ContainsKey(record))
            {
                return memo[record];
            }

            var result = new Dictionary<string, int>();
            for (int i = 0; i < record.FieldCount; i++)
            {
                result.Add(record.GetName(i).ToLower(), i);
            }

            memo[record] = result;

            return result;
        }


        public static void PopulateProperties<T>(this IDataReader dataReader, T obj) where T : class
        {
            try
            {
                IEnumerable<PropertyInfo> properties = obj.DbFieldProperties();

                Dictionary<string, int> fields = dataReader.GetColumnOrdinals();

                MethodInfo method = typeof(IDataReaderExtensions).GetMethod("GetValue", new[] { typeof(IDataReader), typeof(string) });

                foreach (PropertyInfo property in properties)
                {
                    DbFieldAttribute dbField = property.GetDbField();

                    if (fields.ContainsKey(dbField.Field.ToLower()) == false)
                        continue;

                    MethodInfo generic = method.MakeGenericMethod(property.PropertyType);
                    object result = generic.Invoke(null, new object[] { dataReader, dbField.Field });
                    
                    property.SetValue(obj, result);
                }
            }
            catch (TargetInvocationException iex)
            {
                if (iex.InnerException != null && iex.InnerException.GetType() == typeof(InvalidCastException))
                {
                    throw iex.InnerException;
                }
                else
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Attempts to populate an object from the database with DbField attributes set.
        /// </summary>
        /// <typeparam name="T">Object Type to Populate</typeparam>
        /// <param name="dataReader">Data Reader</param>
        /// <returns></returns>
        private static T GetObject<T>(this IDataReader dataReader) where T : class
        {
            T obj = (T)Activator.CreateInstance(typeof(T));

            dataReader.PopulateProperties(obj);

            return obj;
        }


        /// <summary>
        /// Attempts to populate an object from the database with DbField attributes set.
        /// </summary>
        /// <typeparam name="T">Object Type to Populate</typeparam>
        /// <param name="dataReader">Data Reader</param>
        /// <returns></returns>
        public static List<T> GetObjects<T>(this IDataReader dataReader) where T : class
        {
            var result = new List<T>();

            while (dataReader.Read())
            {
                T obj = dataReader.GetObject<T>();
                if (obj != null)
                    result.Add(obj);
            }

            return result;
        }


        /// <summary>
        /// Attempts to populate an object from the database with DbField attributes set.
        /// </summary>
        /// <typeparam name="T">Object type to Populate</typeparam>
        /// <param name="dataReader">Data Reader</param>
        /// <returns></returns>
        public static T GetSingleObject<T>(this IDataReader dataReader) where T : class
        {
            T obj = null;

            if (dataReader.Read())
            {
                obj = dataReader.GetObject<T>();
            }

            return obj;
        }
    }
}
