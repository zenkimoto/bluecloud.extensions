using System;
using System.Reflection;
using System.Data;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BlueCloud.Extensions.Assembly;
using BlueCloud.Extensions.Collections;

namespace BlueCloud.Extensions.Data
{
    public static class IDbConnectionExtensions
    {

        #region IDbCommand Factory Methods

        /// <summary>
        /// Creates a new IDbCommand with an initialized SQL Statement.
        /// </summary>
        /// <param name="sqlString">SQL String to Execute</param>
        public static IDbCommand CommandWithSqlString(this IDbConnection connection, string sqlString)
        {
            IDbCommand command = connection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = sqlString;

            command.GetType().GetProperty("BindByName")?.SetValue(command, true);

            return command;
        }


        /// <summary>
        /// Creates a new IDbCommand with an initialized Stored Procedure Name.
        /// </summary>
        /// <param name="sqlString">Stored Procedure to Execute</param>
        public static IDbCommand CommandWithStoredProcedure(this IDbConnection connection, string storedProcedure)
        {
            IDbCommand command = CommandWithSqlString(connection, storedProcedure);

            command.CommandType = CommandType.StoredProcedure;

            return command;
        }


        /// <summary>
        /// Commands the with embedded resource.
        /// </summary>
        /// <returns>The with embedded resource.</returns>
        /// <param name="connection">Connection.</param>
        /// <param name="embeddedResource">Embedded resource.</param>
        public static IDbCommand CommandWithEmbeddedResource(this IDbConnection connection, string embeddedResource)
        {
            return connection.CommandWithEmbeddedResource(embeddedResource, System.Reflection.Assembly.GetCallingAssembly());
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns>The with embedded resource.</returns>
        /// <param name="connection">Connection.</param>
        /// <param name="embeddedResource">Embedded resource.</param>
        /// <param name="assembly">Assembly.</param>
        public static IDbCommand CommandWithEmbeddedResource(this IDbConnection connection, string embeddedResource, System.Reflection.Assembly assembly)
        {
            string sql = assembly.GetEmbeddedResourceString(embeddedResource);

            return connection.CommandWithSqlString(sql);
        }

        #endregion


        #region Execute Query Methods

        /// <summary>
        /// Executes the query string.
        /// </summary>
        /// <param name="connection">Connection.</param>
        /// <param name="sqlString">Sql string.</param>
        /// <param name="readerCallback">Reader callback.</param>
        /// <param name="validateParameters">If set to <c>true</c> validate parameters.</param>
        public static void ExecuteQueryString(this IDbConnection connection, string sqlString, Action<IDataReader> readerCallback, bool validateParameters = true) {
            ExecuteQueryString(connection, sqlString, null, readerCallback, validateParameters);
        }

        /// <summary>
        /// Executes a Non Query SQL Statement.  An exception will be thrown if the connection is not open.
        /// </summary>
        /// <param name="sqlString">Query SQL String to Execute</param>
        /// <param name="commandCallback">Parameter lambda expression callback in the form (command) => { }</param>
        /// <returns></returns>
        public static void ExecuteQueryString(this IDbConnection connection, string sqlString, Action<IDbCommand> commandCallback, Action<IDataReader> readerCallback, bool validateParameters = true)
        {
            if (connection.State != ConnectionState.Open)
                throw new InvalidOperationException("Connection must be open to call ExecuteQueryString / ExecuteQueryEmbeddedResource");
            if (sqlString == null)
                throw new ArgumentNullException(nameof(sqlString));
            if (readerCallback == null)
                throw new ArgumentNullException(nameof(readerCallback));

            using (IDbCommand command = connection.CommandWithSqlString(sqlString))
            {
                commandCallback?.Invoke(command);
#if DEBUG
                if (validateParameters)
                {
                    command.ValidateParameters();
                }
#endif
                using (var reader = command.ExecuteReader()) {
                    readerCallback(reader);
                    reader.Close();
                }
            }
        }


        /// <summary>
        /// Executes a Query SQL Statement in an Embedded Resource.  An exception will be thrown if the connection is not open.
        /// </summary>
        /// <param name="embeddedResource">Embedded Resource Name</param>
        /// <param name="assembly">Assembly Where Embedded Resource Resides</param>
        /// <param name="commandCallback">Parameter lambda expression callback in the form (command) => { }</param>
        /// <returns></returns>
        public static void ExecuteQueryEmbeddedResource(this IDbConnection connection, string embeddedResource, System.Reflection.Assembly assembly, Action<IDbCommand> commandCallback, Action<IDataReader> readerCallback, bool validateParameters = true)
        {
            string sql = assembly.GetEmbeddedResourceString(embeddedResource);
            connection.ExecuteQueryString(sql, commandCallback, readerCallback, validateParameters);
        }


        /// <summary>
        /// Executes the query embedded resource.
        /// </summary>
        /// <param name="connection">Connection.</param>
        /// <param name="embeddedResource">Embedded resource.</param>
        /// <param name="assembly">Assembly.</param>
        /// <param name="readerCallback">Reader callback.</param>
        /// <param name="validateParameters">If set to <c>true</c> validate parameters.</param>
        public static void ExecuteQueryEmbeddedResource(this IDbConnection connection, string embeddedResource, System.Reflection.Assembly assembly, Action<IDataReader> readerCallback, bool validateParameters = true)
        {
            string sql = assembly.GetEmbeddedResourceString(embeddedResource);
            connection.ExecuteQueryString(sql, null, readerCallback, validateParameters);
        }


        /// <summary>
        /// Executes a Query SQL Statement in an Embedded Resource.  An exception will be thrown if the connection is not open.
        /// </summary>
        /// <returns>The query embedded resource.</returns>
        /// <param name="connection">Connection.</param>
        /// <param name="embeddedResource">Embedded resource.</param>
        /// <param name="commandCallback">Command callback.</param>
        /// <param name="validateParameters">If set to <c>true</c> validate parameters.</param>
        public static void ExecuteQueryEmbeddedResource(this IDbConnection connection, string embeddedResource, Action<IDbCommand> commandCallback, Action<IDataReader> readerCallback, bool validateParameters = true)
        {
            connection.ExecuteQueryEmbeddedResource(embeddedResource, System.Reflection.Assembly.GetCallingAssembly(), commandCallback, readerCallback, validateParameters);
        }


        /// <summary>
        /// Executes the query embedded resource.
        /// </summary>
        /// <param name="connection">Connection.</param>
        /// <param name="embeddedResource">Embedded resource.</param>
        /// <param name="readerCallback">Reader callback.</param>
        /// <param name="validateParameters">If set to <c>true</c> validate parameters.</param>
        public static void ExecuteQueryEmbeddedResource(this IDbConnection connection, string embeddedResource, Action<IDataReader> readerCallback, bool validateParameters = true)
        {
            connection.ExecuteQueryEmbeddedResource(embeddedResource, System.Reflection.Assembly.GetCallingAssembly(), null, readerCallback, validateParameters);
        }


        /// <summary>
        /// Executes the query for object.
        /// </summary>
        /// <returns>The query for object.</returns>
        /// <param name="connection">Connection.</param>
        /// <param name="sqlString">Sql string.</param>
        /// <param name="commandCallback">Command callback.</param>
        /// <param name="readerCallback">Reader callback.</param>
        /// <param name="validateParameters">If set to <c>true</c> validate parameters.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static T ExecuteQueryForObject<T>(this IDbConnection connection, string sqlString, Action<IDbCommand> commandCallback, Func<IDataReader, T> readerCallback, bool validateParameters = true) {
            if (connection.State != ConnectionState.Open)
                throw new InvalidOperationException("Connection must be open to call ExecuteQueryString / ExecuteQueryEmbeddedResource");
            if (sqlString == null)
                throw new ArgumentNullException(nameof(sqlString));
            if (readerCallback == null)
                throw new ArgumentNullException(nameof(readerCallback));

            using (IDbCommand command = connection.CommandWithSqlString(sqlString))
            {
                commandCallback?.Invoke(command);
#if DEBUG
                if (validateParameters)
                {
                    command.ValidateParameters();
                }
#endif
                using (var reader = command.ExecuteReader())
                {
                    T result = default(T);
                    if (reader.Read()) {
                        result = readerCallback(reader);
                    }
                    reader.Close();
                    return result;
                }
            }
        }


        /// <summary>
        /// Executes the query for object.
        /// </summary>
        /// <returns>The query for object.</returns>
        /// <param name="connection">Connection.</param>
        /// <param name="sqlString">Sql string.</param>
        /// <param name="readerCallback">Reader callback.</param>
        /// <param name="validateParameters">If set to <c>true</c> validate parameters.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static T ExecuteQueryForObject<T>(this IDbConnection connection, string sqlString, Func<IDataReader, T> readerCallback, bool validateParameters = true) {
            return connection.ExecuteQueryForObject<T>(sqlString, null, readerCallback, validateParameters);
        }


        /// <summary>
        /// Executes the query for objects.
        /// </summary>
        /// <returns>The query for objects.</returns>
        /// <param name="connection">Connection.</param>
        /// <param name="sqlString">Sql string.</param>
        /// <param name="commandCallback">Command callback.</param>
        /// <param name="readerCallback">Reader callback.</param>
        /// <param name="validateParameters">If set to <c>true</c> validate parameters.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static IEnumerable<T> ExecuteQueryForObjects<T>(this IDbConnection connection, string sqlString, Action<IDbCommand> commandCallback, Func<IDataReader, T> readerCallback, bool validateParameters = true)
        {
            if (connection.State != ConnectionState.Open)
                throw new InvalidOperationException("Connection must be open to call ExecuteQueryString / ExecuteQueryEmbeddedResource");
            if (sqlString == null)
                throw new ArgumentNullException(nameof(sqlString));
            if (readerCallback == null)
                throw new ArgumentNullException(nameof(readerCallback));

            using (IDbCommand command = connection.CommandWithSqlString(sqlString))
            {
                commandCallback?.Invoke(command);
#if DEBUG
                if (validateParameters)
                {
                    command.ValidateParameters();
                }
#endif
                using (var reader = command.ExecuteReader())
                {
                    var result = new List<T>();
                    while (reader.Read()) {
                        T obj = readerCallback(reader);
                        result.Add(obj);
                    }
                    reader.Close();
                    return result;
                }
            }
        }


        /// <summary>
        /// Executes the query for objects.
        /// </summary>
        /// <returns>The query for objects.</returns>
        /// <param name="connection">Connection.</param>
        /// <param name="sqlString">Sql string.</param>
        /// <param name="readerCallback">Reader callback.</param>
        /// <param name="validateParameters">If set to <c>true</c> validate parameters.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static IEnumerable<T> ExecuteQueryForObjects<T>(this IDbConnection connection, string sqlString, Func<IDataReader, T> readerCallback, bool validateParameters = true) {
            return connection.ExecuteQueryForObjects(sqlString, null, readerCallback, validateParameters);
        }

        #endregion


        #region Execute Non Query Methods

        /// <summary>
        /// Executes a Non Query SQL Statement.  An exception will be thrown if the connection is not open.
        /// </summary>
        /// <param name="nonQuerySqlString">Non Query SQL String to Execute</param>
        /// <param name="commandCallback">Parameter lambda expression callback in the form (command) => { }</param>
        public static int ExecuteNonQueryString(this IDbConnection connection, string nonQuerySqlString, Action<IDbCommand> commandCallback = null, bool validateParameters = true)
        {
            if (connection.State != ConnectionState.Open)
                throw new InvalidOperationException("Connection must be open to call ExecuteNonQueryString / ExecuteNonQueryEmbeddedResource");
            if (nonQuerySqlString == null)
                throw new ArgumentNullException(nameof(nonQuerySqlString));

            using (IDbCommand command = connection.CommandWithSqlString(nonQuerySqlString))
            {
                commandCallback?.Invoke(command);
#if DEBUG
                if (validateParameters)
                {
                    command.ValidateParameters();
                }
#endif
                return command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Executes a Non Query SQL Statement in an Embedded Resource.  An exception will be thrown if the connection is not open.
        /// </summary>
        /// <param name="embeddedResource">Embedded Resource Name</param>
        /// <param name="commandCallback">Parameter lambda expression callback in the form (command) => { }</param>
        /// <returns></returns>
        public static int ExecuteNonQueryEmbeddedResource(this IDbConnection connection, string embeddedResource, Action<IDbCommand> commandCallback = null, bool validateParameters = true)
        {
            return connection.ExecuteNonQueryEmbeddedResource(embeddedResource, System.Reflection.Assembly.GetCallingAssembly(), commandCallback, validateParameters);
        }


        /// <summary>
        /// Executes a Non Query SQL Statement in an Embedded Resource.  An exception will be thrown if the connection is not open.
        /// </summary>
        /// <param name="embeddedResource">Embedded Resource Name</param>
        /// <param name="assembly">Assembly Where Embedded Resource Resides</param>
        /// <param name="commandCallback">Parameter lambda expression callback in the form (command) => { }</param>
        /// <returns></returns>
        public static int ExecuteNonQueryEmbeddedResource(this IDbConnection connection, string embeddedResource, System.Reflection.Assembly assembly, Action<IDbCommand> commandCallback = null, bool validateParameters = true)
        {
            string sql = assembly.GetEmbeddedResourceString(embeddedResource);

            return connection.ExecuteNonQueryString(sql, commandCallback, validateParameters);
        }


        /// <summary>
        /// Executes a Non Query SQL Statement for a DbField decorated object.  Sql Parameters will be automatically populated.
        /// </summary>
        /// <typeparam name="T">Associated Object Type</typeparam>
        /// <param name="connection">IDbConnection</param>
        /// <param name="nonQuerySqlString">Non Query SQL String</param>
        /// <param name="obj">Object from which parameters are to be populated</param>
        /// <returns></returns>
        public static int ExecuteNonQueryStringForObject<T>(this IDbConnection connection, string nonQuerySqlString, T obj, bool validateParameters = true)
        {
            return connection.ExecuteNonQueryString(nonQuerySqlString, (command) =>
            {
                List<string> bindParameters = command.ParameterNamesFromCommandText().Map(x => x.TrimStart(':', '@').ToLower());

                PropertyInfo[] properties = typeof(T).GetProperties();

                foreach (PropertyInfo property in properties)
                {
                    var dbField = (DbFieldAttribute)property.GetCustomAttribute(typeof(DbFieldAttribute), true);

                    if (dbField == null)
                        continue;

                    if (dbField.SqlParameterName == null)
                        continue;

                    if (bindParameters.Contains(dbField.SqlParameterName.TrimStart(':', '@').ToLower()) == false)
                        continue;

                    Debug.WriteLine(property.Name.ToString() + " [" + property.PropertyType.ToString() + "]" + " <-> Database Field: " + dbField.Field);

                    if (property.PropertyType == typeof(Boolean))
                    {
                        command.AddParameter(dbField.SqlParameterName, (bool)property.GetValue(obj) ? 1 : 0);
                    }
                    else
                    {
                        command.AddParameter(dbField.SqlParameterName, property.GetValue(obj));
                    }

                }
            }, validateParameters);
        }


        /// <summary>
        /// Executes a Non Query SQL Embedded Resource for a DbField decorated object.  Sql Parameters will be automatically populated.
        /// </summary>
        /// <typeparam name="T">Associated Object Type</typeparam>
        /// <param name="connection">IDbConnection</param>
        /// <param name="embeddedResource">Embedded Resource Name</param>
        /// <param name="obj">Object from which parameters are to be populated</param>
        /// <returns></returns>
        public static int ExecuteNonQueryEmbeddedResourceForObject<T>(this IDbConnection connection, string embeddedResource, T obj, bool validateParameters = true)
        {
            return ExecuteNonQueryEmbeddedResourceForObject(connection, embeddedResource, System.Reflection.Assembly.GetCallingAssembly(), obj, validateParameters);
        }


        /// <summary>
        /// Executes a Non Query SQL Embedded Resource for a DbField decorated object.  Sql Parameters will be automatically populated.
        /// </summary>
        /// <typeparam name="T">Associated Object Type</typeparam>
        /// <param name="connection">IDbConnection</param>
        /// <param name="embeddedResource">Embedded Resource Name</param>
        /// <param name="assembly">Assembly Where Embedded Resource is Located</param>
        /// <param name="obj">Object from which parameters are to be populated</param>
        /// <returns></returns>
        public static int ExecuteNonQueryEmbeddedResourceForObject<T>(this IDbConnection connection, string embeddedResource, System.Reflection.Assembly assembly, T obj, bool validateParameters = true)
        {
            string sql = assembly.GetEmbeddedResourceString(embeddedResource);

            return connection.ExecuteNonQueryStringForObject(sql, obj, validateParameters);
        }

        #endregion


        #region Execute Scalar

        /// <summary>
        /// Executes a Query SQL statement, returning a scalar value. An exception will be thrown if the connection is not open.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="sqlString"></param>
        /// <param name="commandCallback"></param>
        /// <returns></returns>
        public static object ExecuteQueryScalar(this IDbConnection connection, string sqlString, Action<IDbCommand> commandCallback = null, bool validateParameters = true)
        {
            if (connection.State != ConnectionState.Open)
                throw new InvalidOperationException("Connection must be open to call ExecuteQueryString / ExecuteQueryEmbeddedResource");
            if (sqlString == null)
                throw new ArgumentNullException(nameof(sqlString));

            using (IDbCommand command = connection.CommandWithSqlString(sqlString))
            {
                commandCallback?.Invoke(command);
#if DEBUG
                if (validateParameters)
                {
                    command.ValidateParameters();
                }
#endif
                return command.ExecuteScalar();
            }
        }


        /// <summary>
        /// Executes a Query SQL statement, returning a scalar value. An exception will be thrown if the connection is not open.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="embeddedResource"></param>
        /// <param name="assembly"></param>
        /// <param name="commandCallback"></param>
        /// <returns></returns>
        public static object ExecuteScalarEmbeddedResource(this IDbConnection connection, string embeddedResource, System.Reflection.Assembly assembly, Action<IDbCommand> commandCallback = null, bool validateParameters = true)
        {
            string sql = assembly.GetEmbeddedResourceString(embeddedResource);
            return connection.ExecuteQueryScalar(sql, commandCallback, validateParameters);
        }


        /// <summary>
        /// Executes a Query SQL statement, returning a scalar value. An exception will be thrown if the connection is not open.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="embeddedResource"></param>
        /// <param name="commandCallback"></param>
        /// <returns></returns>
        public static object ExecuteScalarEmbeddedResource(this IDbConnection connection, string embeddedResource, Action<IDbCommand> commandCallback = null, bool validateParameters = true)
        {
            return connection.ExecuteScalarEmbeddedResource(embeddedResource, System.Reflection.Assembly.GetExecutingAssembly(), commandCallback, validateParameters);
        }

        #endregion


        #region Get Objects Methods


        /// <summary>
        /// Gets the objects from embedded resource.
        /// </summary>
        /// <returns>The objects from embedded resource.</returns>
        /// <param name="connection">Connection.</param>
        /// <param name="embeddedResource">Embedded resource.</param>
        /// <param name="commandCallback">Command callback.</param>
        /// <param name="validateParameters">If set to <c>true</c> validate parameters.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static IEnumerable<T> GetObjectsFromEmbeddedResource<T>(this IDbConnection connection, string embeddedResource, Action<IDbCommand> commandCallback = null, bool validateParameters = true) where T : class
        {
            return connection.GetObjectsFromEmbeddedResource<T>(embeddedResource, System.Reflection.Assembly.GetCallingAssembly(), commandCallback, validateParameters);
        }


        /// <summary>
        /// Gets the objects from embedded resource.
        /// </summary>
        /// <returns>The objects from embedded resource.</returns>
        /// <param name="connection">Connection.</param>
        /// <param name="embeddedResource">Embedded resource.</param>
        /// <param name="assembly">Assembly.</param>
        /// <param name="commandCallback">Command callback.</param>
        /// <param name="validateParameters">If set to <c>true</c> validate parameters.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static IEnumerable<T> GetObjectsFromEmbeddedResource<T>(this IDbConnection connection, string embeddedResource, System.Reflection.Assembly assembly, Action<IDbCommand> commandCallback = null, bool validateParameters = true) where T : class
        {
            IEnumerable<T> result = null;

            connection.ExecuteQueryEmbeddedResource(embeddedResource, assembly, commandCallback, reader => 
            {
                result = reader.MapToObjects<T>();
            }, validateParameters);

            return result;
        }


        /// <summary>
        /// Gets the objects from query string.
        /// </summary>
        /// <returns>The objects from query string.</returns>
        /// <param name="connection">Connection.</param>
        /// <param name="sqlString">Sql string.</param>
        /// <param name="commandCallback">Command callback.</param>
        /// <param name="validateParameters">If set to <c>true</c> validate parameters.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static IEnumerable<T> GetObjectsFromQueryString<T>(this IDbConnection connection, string sqlString, Action<IDbCommand> commandCallback = null, bool validateParameters = true) where T : class
        {
            IEnumerable<T> result = null;

            connection.ExecuteQueryString(sqlString, commandCallback, reader =>
            {
                result = reader.MapToObjects<T>();
            }, validateParameters);

            return result;
        }


        /// <summary>
        /// Executes the non query embedded resource for objects.
        /// </summary>
        /// <param name="connection">Connection.</param>
        /// <param name="embeddedResource">Embedded resource.</param>
        /// <param name="objects">Objects.</param>
        /// <param name="validateParameters">If set to <c>true</c> validate parameters.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static void ExecuteNonQueryEmbeddedResourceForObjects<T>(this IDbConnection connection, string embeddedResource, List<T> objects, bool validateParameters = true) where T : class
        {
            foreach (T obj in objects)
            {
                connection.ExecuteNonQueryEmbeddedResourceForObject(embeddedResource, obj, validateParameters);
            }
        }


        /// <summary>
        /// Executes the non query embedded resource for objects.
        /// </summary>
        /// <param name="connection">Connection.</param>
        /// <param name="embeddedResource">Embedded resource.</param>
        /// <param name="assembly">Assembly.</param>
        /// <param name="objects">Objects.</param>
        /// <param name="validateParameters">If set to <c>true</c> validate parameters.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static void ExecuteNonQueryEmbeddedResourceForObjects<T>(this IDbConnection connection, string embeddedResource, System.Reflection.Assembly assembly, List<T> objects, bool validateParameters = true) where T : class
        {
            foreach (T obj in objects)
            {
                connection.ExecuteNonQueryEmbeddedResourceForObject(embeddedResource, assembly, obj, validateParameters);
            }
        }

        #endregion


        #region Get Single Object Methods

        /// <summary>
        /// Gets the single object from embedded resource.
        /// </summary>
        /// <returns>The single object from embedded resource.</returns>
        /// <param name="connection">Connection.</param>
        /// <param name="embeddedResource">Embedded resource.</param>
        /// <param name="commandCallback">Command callback.</param>
        /// <param name="validateParameters">If set to <c>true</c> validate parameters.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static T GetSingleObjectFromEmbeddedResource<T>(this IDbConnection connection, string embeddedResource, Action<IDbCommand> commandCallback = null, bool validateParameters = true) where T : class
        {
            return connection.GetSingleObjectFromEmbeddedResource<T>(embeddedResource, System.Reflection.Assembly.GetCallingAssembly(), commandCallback, validateParameters);
        }

        public static T GetSingleObjectFromEmbeddedResource<T>(this IDbConnection connection, string embeddedResource, System.Reflection.Assembly assembly, Action<IDbCommand> commandCallback = null, bool validateParameters = true) where T : class
        {
            T obj = null;

            connection.ExecuteQueryEmbeddedResource(embeddedResource, assembly, commandCallback, reader =>
            {
                obj = reader.MapToObjects<T>(1).First();
            }, validateParameters);

            return obj;
        }

        public static T GetSingleObjectFromQueryString<T>(this IDbConnection connection, string sqlString, Action<IDbCommand> commandCallback = null, bool validateParameters = true) where T : class
        {
            T obj = null;

            connection.ExecuteQueryString(sqlString, commandCallback, reader =>
            {
                obj = reader.MapToObjects<T>(1).First();
            }, validateParameters);

            return obj;
        }

        #endregion
    }
}