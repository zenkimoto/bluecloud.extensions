using System;
using System.Data;
using System.Collections.Generic;
using System.Reflection;

namespace BlueCloud.Extensions.Data
{
    public static class IDataReaderExtensions
    {
        /// <summary>
        /// Returns a value from the database with a given type.
        /// </summary>
        /// <typeparam name="T">Data Type</typeparam>
        /// <param name="fieldName">Database Field Name</param>
        /// <returns>Value from database</returns>
        public static T GetValue<T>(this IDataReader dataReader, string fieldName)
        {
            if (fieldName == null)
                throw new ArgumentNullException(nameof(fieldName));

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
                        throw new InvalidOperationException($"Attempting to assign null to a non-nullable type for field: '{fieldName}'");
                    }

                    return default(T);
                }

                value = dataReader.GetValue(ordinal);

                T result = isNullableType ? (T)Convert.ChangeType(value, underlyingType) : (T)Convert.ChangeType(value, type);

                // Handle DateTimes
                if (type == typeof(DateTime))
                {
                    result = (T)Convert.ChangeType(result, typeof(DateTime));
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


        #region Mapper Functions

        /// <summary>
        /// Maps a data reader result to model classes
        /// </summary>
        /// <returns>Mapped Model Objects</returns>
        /// <param name="dataReader">Data reader</param>
        /// <typeparam name="T">Data Type</typeparam>
        public static IEnumerable<T> MapToObjects<T>(this IDataReader dataReader) where T : class
        {
            return dataReader.MapToObjects<T>(-1);
        }


        /// <summary>
        /// Maps a data reader result to model classes
        /// 
        /// Amount of records to map. A negative 1 (-1) will map all records.
        /// </summary>
        /// <returns>Mapped Model Objects</returns>
        /// <param name="dataReader">Data reader</param>
        /// <param name="take">Amount of records to map. A negative 1 (-1) will map all records.</param>
        /// <typeparam name="T">Data Type</typeparam>
        public static IEnumerable<T> MapToObjects<T>(this IDataReader dataReader, int take) where T : class
        {
            var dbProperties = GetDatabaseProperties<T>();
            var objects = new List<T>();

            for (int i = 0; dataReader.Read() && (i < take || take == -1); i++)
            {
                T obj = dataReader.MapToObject<T>(dbProperties);

                objects.Add(obj);
            }

            return objects;
        }


        /// <summary>
        /// Maps a single database row to an object.
        /// </summary>
        /// <returns>The to object.</returns>
        /// <param name="dataReader">Data reader</param>
        /// <param name="dbProperties">Cached Mapping Properties</param>
        /// <typeparam name="T">Data Type</typeparam>
        private static T MapToObject<T>(this IDataReader dataReader, List<Tuple<string, PropertyInfo, bool>> dbProperties) {
            T obj = (T)Activator.CreateInstance(typeof(T));

            foreach (var mapping in dbProperties)
            {
                object databaseValue = null;

                try
                {
                    databaseValue = dataReader[mapping.Item1];
                }
                catch (ArgumentOutOfRangeException ex)
                {
                    var errorMessage = $"The database field: '{mapping.Item1}' specified in the DbField attribute does not exist in query result.";
                    throw new InvalidOperationException(errorMessage, ex);
                }

                if (databaseValue == DBNull.Value)
                {
                    databaseValue = null;
                }
                else if (mapping.Item2.PropertyType == typeof(DateTime))
                {
                    databaseValue = (DateTime)Convert.ChangeType(databaseValue, typeof(DateTime));
                }

                // Allow for custom user mapping
                if (obj is IDbHydrationOverridable && ((IDbHydrationOverridable)obj).ShouldOverridePropertyHydration(mapping.Item2.Name))
                {
                    databaseValue = ((IDbHydrationOverridable)obj).OverridePropertyHydration(mapping.Item2.Name, databaseValue);
                }

                if (databaseValue == null && mapping.Item3 == false)
                {
                    var errorMessage = $"Attempting to assign NULL in database field: '{mapping.Item1}' to a non-nullable property: '{mapping.Item2.Name}'.";
                    throw new InvalidCastException(errorMessage);
                }

                try
                {
                    mapping.Item2.SetValue(obj, databaseValue);
                }
                catch (ArgumentException ex)
                {
                    var errorMessage = $"Unable to convert database field: '{mapping.Item1}' to property: '{mapping.Item2.Name}'. Detail: {ex.Message}";
                    throw new InvalidCastException(errorMessage, ex);
                }
            }

            return obj;
        }


        /// <summary>
        /// Memoize tuple information.
        /// </summary>
        private static readonly Dictionary<string, List<Tuple<string, PropertyInfo, bool>>> tupleMemo = new Dictionary<string, List<Tuple<string, PropertyInfo, bool>>>();


        /// <summary>
        /// Get all mapped properties of type T.  (Properties with the DbField attribute)
        /// Returns a list of tuples with the following:
        ///   ( Database Field Name, Reflected Property, If the property is a nullable type )
        /// </summary>
        /// <typeparam name="T">Data Type</typeparam>
        /// <returns>List of Tuples</returns>
        private static List<Tuple<string, PropertyInfo, bool>> GetDatabaseProperties<T>()
        {
            var type = typeof(T);

            if (tupleMemo.ContainsKey(type.FullName))
                return tupleMemo[type.FullName];

            var properties = type.GetProperties();
            var result = new List<Tuple<string, PropertyInfo, bool>>();

            foreach (var property in properties)
            {
                var attrib = property.GetCustomAttribute<DbFieldAttribute>(true);

                if (attrib != null)
                {
                    var isNullable = Nullable.GetUnderlyingType(property.PropertyType) != null;
                    var tuple = new Tuple<string, PropertyInfo, bool>(attrib.Field, property, isNullable);
                    result.Add(tuple);
                }
            }

            tupleMemo[type.FullName] = result;

            return result;
        }

        #endregion


        #region Helper Functions

        /// <summary>
        /// Returns a dictionary mapping of a database field name to column ordinal.
        /// </summary>
        /// <returns>Mapping of field name to column ordinal</returns>
        /// <param name="record">Data Record</param>
        public static Dictionary<string, int> GetColumnOrdinals(this IDataRecord record)
        {
            var result = new Dictionary<string, int>();
            for (int i = 0; i < record.FieldCount; i++)
            {
                result.Add(record.GetName(i).ToLower(), i);
            }

            return result;
        }

        #endregion
    }
}
