using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using BlueCloud.Extensions.Assembly;

namespace BlueCloud.Extensions.Data
{
    public static class IDbConnectionExtensions
    {
        #region IDbCommand Factory Methods

        /// <summary>
        /// Creates a new IDbCommand with an initialized SQL Statement.
        /// </summary>
        /// <returns>IDbCommand</returns>
        /// <param name="connection">IDbConnection</param>
        /// <param name="sqlString">SQL String to Execute</param>
        public static IDbCommand CommandWithSqlString(this IDbConnection connection, string sqlString)
        {
            IDbCommand command = connection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = sqlString ?? throw new ArgumentNullException(nameof(sqlString));

            command.GetType().GetProperty("BindByName")?.SetValue(command, true);

            return command;
        }


        /// <summary>
        /// Creates a new IDbCommand with an initialized Stored Procedure Name.
        /// </summary>
        /// <returns>IDbCommand</returns>
        /// <param name="connection">IDbConnection</param>
        /// <param name="storedProcedure">Stored Procedure to Execute</param>
        public static IDbCommand CommandWithStoredProcedure(this IDbConnection connection, string storedProcedure)
        {
            IDbCommand command = CommandWithSqlString(connection, storedProcedure);

            command.CommandType = CommandType.StoredProcedure;

            return command;
        }


        /// <summary>
        /// Creates a new IDbCommand with an initialized from a specifed embedded resource.
        /// </summary>
        /// <returns>IDbCommand</returns>
        /// <param name="connection">IDbConnection</param>
        /// <param name="embeddedResource">Embedded resource name</param>
        public static IDbCommand CommandWithEmbeddedResource(this IDbConnection connection, string embeddedResource)
        {
            return connection.CommandWithEmbeddedResource(embeddedResource, System.Reflection.Assembly.GetCallingAssembly());
        }


        /// <summary>
        /// Creates a new IDbCommand with an initialized from a specifed embedded resource.
        /// </summary>
        /// <returns>The with embedded resource.</returns>
        /// <param name="connection">IDbConnection</param>
        /// <param name="embeddedResource">Embedded resource name</param>
        /// <param name="assembly">Assembly where embedded resource exists</param>
        public static IDbCommand CommandWithEmbeddedResource(this IDbConnection connection, string embeddedResource, System.Reflection.Assembly assembly)
        {
            if (embeddedResource == null)
                throw new ArgumentNullException(nameof(embeddedResource));
            if (assembly == null)
                throw new ArgumentNullException(nameof(assembly));

            string sql = assembly.GetEmbeddedResourceString(embeddedResource);

            return connection.CommandWithSqlString(sql);
        }

        #endregion


        #region Execute Query Methods

        /// <summary>
        /// Executes a Query SQL Statement.  An exception will be thrown if the connection is not open.
        /// </summary>
        /// <param name="connection">IDbConnection</param>
        /// <param name="sqlString">Sql string to execute</param>
        /// <param name="readerCallback">Reader callback</param>
        /// <param name="validateParameters">If set to <c>true</c> validate parameters.</param>
        public static void ExecuteQueryString(this IDbConnection connection, string sqlString, Action<IDataReader> readerCallback, bool validateParameters = false)
        {
            ExecuteQueryString(connection, sqlString, null, readerCallback, validateParameters);
        }


        /// <summary>
        /// Executes a Query SQL Statement.  An exception will be thrown if the connection is not open.
        /// </summary>
        /// <param name="connection">IDbConnection</param>
        /// <param name="sqlString">Query SQL String to Execute</param>
        /// <param name="commandCallback">Parameter lambda expression callback in the form (command) => { }</param>
        /// <param name="readerCallback">Reader lambda expression callback in the form (reader) => { }</param>
        /// <param name="validateParameters">If set to <c>true</c> validate parameters.</param>
        public static void ExecuteQueryString(this IDbConnection connection, string sqlString, Action<IDbCommand> commandCallback, Action<IDataReader> readerCallback, bool validateParameters = false)
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

                if (validateParameters)
                {
                    command.ValidateParameters();
                }

                using (var reader = command.ExecuteReader())
                {
                    readerCallback(reader);
                    reader.Close();
                }
            }
        }


        /// <summary>
        /// Executes a Query SQL Statement in an Embedded Resource.  An exception will be thrown if the connection is not open.
        /// </summary>
        /// <param name="connection">IDbConnection</param>
        /// <param name="embeddedResource">Embedded Resource Name</param>
        /// <param name="assembly">Assembly Where Embedded Resource Resides</param>
        /// <param name="commandCallback">Parameter lambda expression callback in the form (command) => { }</param>
        /// <param name="readerCallback">Reader lambda expression callback in the form (reader) => { }</param>
        /// <param name="validateParameters">If set to <c>true</c> validate parameters.</param>
        public static void ExecuteQueryEmbeddedResource(this IDbConnection connection, string embeddedResource, System.Reflection.Assembly assembly, Action<IDbCommand> commandCallback, Action<IDataReader> readerCallback, bool validateParameters = false)
        {
            if (embeddedResource == null)
                throw new ArgumentNullException(nameof(embeddedResource));

            string sql = assembly.GetEmbeddedResourceString(embeddedResource);
            connection.ExecuteQueryString(sql, commandCallback, readerCallback, validateParameters);
        }


        /// <summary>
        /// Executes a Query SQL Statement in an Embedded Resource.  An exception will be thrown if the connection is not open.
        /// </summary>
        /// <param name="connection">IDbConnection</param>
        /// <param name="embeddedResource">Embedded Resource Name</param>
        /// <param name="assembly">Assembly Where Embedded Resource Resides</param>
        /// <param name="readerCallback">Reader lambda expression callback in the form (reader) => { }</param>
        /// <param name="validateParameters">If set to <c>true</c> validate parameters.</param>
        public static void ExecuteQueryEmbeddedResource(this IDbConnection connection, string embeddedResource, System.Reflection.Assembly assembly, Action<IDataReader> readerCallback, bool validateParameters = false)
        {
            string sql = assembly.GetEmbeddedResourceString(embeddedResource);
            connection.ExecuteQueryString(sql, null, readerCallback, validateParameters);
        }


        /// <summary>
        /// Executes a Query SQL Statement in an Embedded Resource.  An exception will be thrown if the connection is not open.
        /// </summary>
        /// <param name="connection">IDbConnection</param>
        /// <param name="embeddedResource">Embedded Resource Name</param>
        /// <param name="commandCallback">Parameter lambda expression callback in the form (command) => { }</param>
        /// <param name="readerCallback">Reader lambda expression callback in the form (reader) => { }</param>
        /// <param name="validateParameters">If set to <c>true</c> validate parameters.</param>
        public static void ExecuteQueryEmbeddedResource(this IDbConnection connection, string embeddedResource, Action<IDbCommand> commandCallback, Action<IDataReader> readerCallback, bool validateParameters = false)
        {
            connection.ExecuteQueryEmbeddedResource(embeddedResource, System.Reflection.Assembly.GetCallingAssembly(), commandCallback, readerCallback, validateParameters);
        }


        /// <summary>
        /// Executes a Query SQL Statement in an Embedded Resource.  An exception will be thrown if the connection is not open.
        /// </summary>
        /// <param name="connection">IDbConnection</param>
        /// <param name="embeddedResource">Embedded Resource Name</param>
        /// <param name="readerCallback">Reader lambda expression callback in the form (reader) => { }</param>
        /// <param name="validateParameters">If set to <c>true</c> validate parameters.</param>
        public static void ExecuteQueryEmbeddedResource(this IDbConnection connection, string embeddedResource, Action<IDataReader> readerCallback, bool validateParameters = false)
        {
            connection.ExecuteQueryEmbeddedResource(embeddedResource, System.Reflection.Assembly.GetCallingAssembly(), null, readerCallback, validateParameters);
        }

        #endregion


        #region Execute Non Query Methods

        /// <summary>
        /// Executes a Non Query SQL Statement.  An exception will be thrown if the connection is not open.
        /// </summary>
        /// <param name="connection">IDbConnection</param>
        /// <param name="nonQuerySqlString">Non Query SQL String to Execute</param>
        /// <param name="commandCallback">Parameter lambda expression callback in the form (command) => { }</param>
        /// <param name="validateParameters">If set to <c>true</c> validate parameters.</param>
        /// <returns>Number of rows affected</returns>
        public static int ExecuteNonQueryString(this IDbConnection connection, string nonQuerySqlString, Action<IDbCommand> commandCallback = null, bool validateParameters = false)
        {
            if (connection.State != ConnectionState.Open)
                throw new InvalidOperationException("Connection must be open to call ExecuteNonQueryString / ExecuteNonQueryEmbeddedResource");
            if (nonQuerySqlString == null)
                throw new ArgumentNullException(nameof(nonQuerySqlString));

            using (IDbCommand command = connection.CommandWithSqlString(nonQuerySqlString))
            {
                commandCallback?.Invoke(command);

                if (validateParameters)
                {
                    command.ValidateParameters();
                }

                return command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Executes a Non Query SQL Statement in an Embedded Resource.  An exception will be thrown if the connection is not open.
        /// </summary>
        /// <param name="connection">IDbConnection</param>
        /// <param name="embeddedResource">Embedded Resource Name</param>
        /// <param name="commandCallback">Parameter lambda expression callback in the form (command) => { }</param>
        /// <param name="validateParameters">If set to <c>true</c> validate parameters.</param>
        /// <returns>Number of rows affected</returns>
        public static int ExecuteNonQueryEmbeddedResource(this IDbConnection connection, string embeddedResource, Action<IDbCommand> commandCallback = null, bool validateParameters = false)
        {
            return connection.ExecuteNonQueryEmbeddedResource(embeddedResource, System.Reflection.Assembly.GetCallingAssembly(), commandCallback, validateParameters);
        }


        /// <summary>
        /// Executes a Non Query SQL Statement in an Embedded Resource.  An exception will be thrown if the connection is not open.
        /// </summary>
        /// <param name="connection">IDbConnection</param>
        /// <param name="embeddedResource">Embedded Resource Name</param>
        /// <param name="assembly">Assembly Where Embedded Resource Resides</param>
        /// <param name="commandCallback">Parameter lambda expression callback in the form (command) => { }</param>
        /// <param name="validateParameters">If set to <c>true</c> validate parameters.</param>
        /// <returns>Number of rows affected</returns>
        public static int ExecuteNonQueryEmbeddedResource(this IDbConnection connection, string embeddedResource, System.Reflection.Assembly assembly, Action<IDbCommand> commandCallback = null, bool validateParameters = false)
        {
            if (embeddedResource == null)
                throw new ArgumentNullException(nameof(embeddedResource));
            if (assembly == null)
                throw new ArgumentNullException(nameof(assembly));

            string sql = assembly.GetEmbeddedResourceString(embeddedResource);

            return connection.ExecuteNonQueryString(sql, commandCallback, validateParameters);
        }

        #endregion


        #region Execute Scalar

        /// <summary>
        /// Executes a Query SQL statement, returning a scalar value. An exception will be thrown if the connection is not open.
        /// </summary>
        /// <param name="connection">IDbConnection</param>
        /// <param name="sqlString">Query SQL String to Execute</param>
        /// <param name="commandCallback">Parameter lambda expression callback in the form (command) => { }</param>
        /// <param name="validateParameters">If set to <c>true</c> validate parameters.</param>
        /// <returns>Scalar value</returns>
        public static object ExecuteQueryScalar(this IDbConnection connection, string sqlString, Action<IDbCommand> commandCallback = null, bool validateParameters = false)
        {
            if (connection.State != ConnectionState.Open)
                throw new InvalidOperationException("Connection must be open to call ExecuteQueryString / ExecuteQueryEmbeddedResource");
            if (sqlString == null)
                throw new ArgumentNullException(nameof(sqlString));

            using (IDbCommand command = connection.CommandWithSqlString(sqlString))
            {
                commandCallback?.Invoke(command);

                if (validateParameters)
                {
                    command.ValidateParameters();
                }

                return command.ExecuteScalar();
            }
        }


        /// <summary>
        /// Executes a Query SQL statement, returning a scalar value. An exception will be thrown if the connection is not open.
        /// </summary>
        /// <param name="connection">IDbConnection</param>
        /// <param name="embeddedResource">Embedded Resource Name</param>
        /// <param name="assembly">Assembly Where Embedded Resource Resides</param>
        /// <param name="commandCallback">Parameter lambda expression callback in the form (command) => { }</param>
        /// <param name="validateParameters">If set to <c>true</c> validate parameters.</param>
        /// <returns>Scalar value</returns>
        public static object ExecuteScalarEmbeddedResource(this IDbConnection connection, string embeddedResource, System.Reflection.Assembly assembly, Action<IDbCommand> commandCallback = null, bool validateParameters = false)
        {
            if (embeddedResource == null)
                throw new ArgumentNullException(nameof(embeddedResource));
            if (assembly == null)
                throw new ArgumentNullException(nameof(assembly));

            string sql = assembly.GetEmbeddedResourceString(embeddedResource);
            return connection.ExecuteQueryScalar(sql, commandCallback, validateParameters);
        }


        /// <summary>
        /// Executes a Query SQL statement, returning a scalar value. An exception will be thrown if the connection is not open.
        /// </summary>
        /// <param name="connection">IDbConnection</param>
        /// <param name="embeddedResource">Embedded Resource Name</param>
        /// <param name="commandCallback">Parameter lambda expression callback in the form (command) => { }</param>
        /// <param name="validateParameters">If set to <c>true</c> validate parameters.</param>
        /// <returns>Scalar value</returns>
        public static object ExecuteScalarEmbeddedResource(this IDbConnection connection, string embeddedResource, Action<IDbCommand> commandCallback = null, bool validateParameters = false)
        {
            return connection.ExecuteScalarEmbeddedResource(embeddedResource, System.Reflection.Assembly.GetExecutingAssembly(), commandCallback, validateParameters);
        }

        #endregion


        #region Get Single Object Methods

        /// <summary>
        /// Gets a single mapped object from a query string.  If there are more than one record, it will just return the first one. 
        /// An exception will be thrown if the connection is not open.
        /// </summary>
        /// <returns>Mapped Object</returns>
        /// <param name="connection">IDbConnection</param>
        /// <param name="sqlString">Query SQL String to Execute</param>
        /// <param name="commandCallback">Parameter lambda expression callback in the form (command) => { }</param>
        /// <param name="validateParameters">If set to <c>true</c> validate parameters.</param>
        /// <typeparam name="T">Data Type</typeparam>
        public static T GetSingleObjectFromQueryString<T>(this IDbConnection connection, string sqlString, Action<IDbCommand> commandCallback = null, bool validateParameters = false) where T : class
        {
            T obj = null;

            connection.ExecuteQueryString(sqlString, commandCallback, reader =>
            {
                obj = reader.MapToObjects<T>(1).First();
            }, validateParameters);

            return obj;
        }


        /// <summary>
        /// Gets a single mapped object from an embedded resouce.  If there are more than one record, it will just return the first one.
        /// An exception will be thrown if the connection is not open.
        /// </summary>
        /// <returns>Mapped Object</returns>
        /// <param name="connection">IDbConnection</param>
        /// <param name="embeddedResource">Embedded Resource Name</param>
        /// <param name="commandCallback">Parameter lambda expression callback in the form (command) => { }</param>
        /// <param name="validateParameters">If set to <c>true</c> validate parameters.</param>
        /// <typeparam name="T">Data Type</typeparam>
        public static T GetSingleObjectFromEmbeddedResource<T>(this IDbConnection connection, string embeddedResource, Action<IDbCommand> commandCallback = null, bool validateParameters = false) where T : class
        {
            return connection.GetSingleObjectFromEmbeddedResource<T>(embeddedResource, System.Reflection.Assembly.GetCallingAssembly(), commandCallback, validateParameters);
        }


        /// <summary>
        /// Gets a single mapped object from an embedded resouce.  If there are more than one record, it will just return the first one.
        /// An exception will be thrown if the connection is not open.
        /// </summary>
        /// <returns>Mapped Object</returns>
        /// <param name="connection">IDbConnection</param>
        /// <param name="embeddedResource">Embedded Resource Name</param>
        /// <param name="assembly">Assembly Where Embedded Resource Resides</param>
        /// <param name="commandCallback">Parameter lambda expression callback in the form (command) => { }</param>
        /// <param name="validateParameters">If set to <c>true</c> validate parameters.</param>
        /// <typeparam name="T">Data Type</typeparam>
        public static T GetSingleObjectFromEmbeddedResource<T>(this IDbConnection connection, string embeddedResource, System.Reflection.Assembly assembly, Action<IDbCommand> commandCallback = null, bool validateParameters = false) where T : class
        {
            if (embeddedResource == null)
                throw new ArgumentNullException(nameof(embeddedResource));
            if (assembly == null)
                throw new ArgumentNullException(nameof(assembly));

            T obj = null;

            connection.ExecuteQueryEmbeddedResource(embeddedResource, assembly, commandCallback, reader =>
            {
                obj = reader.MapToObjects<T>(1).First();
            }, validateParameters);

            return obj;
        }

        #endregion


        #region GetObjects Methods

        /// <summary>
        /// Gets mapped objects from a query string.  An exception will be thrown if the connection is not open.
        /// </summary>
        /// <returns>Enumerable of Mapped Objects</returns>
        /// <param name="connection">IDbConnection</param>
        /// <param name="sqlString">Query SQL String to Execute</param>
        /// <param name="commandCallback">Parameter lambda expression callback in the form (command) => { }</param>
        /// <param name="validateParameters">If set to <c>true</c> validate parameters.</param>
        /// <typeparam name="T">Data Type</typeparam>
        public static IEnumerable<T> GetObjectsFromQueryString<T>(this IDbConnection connection, string sqlString, Action<IDbCommand> commandCallback = null, bool validateParameters = false) where T : class
        {
            IEnumerable<T> result = null;

            connection.ExecuteQueryString(sqlString, commandCallback, reader =>
            {
                result = reader.MapToObjects<T>();
            }, validateParameters);

            return result;
        }


        /// <summary>
        /// Gets mapped objects from an embedded resource.  An exception will be thrown if the connection is not open.
        /// </summary>
        /// <returns>Enumerable of Mapped Objects</returns>
        /// <param name="connection">IDbConnection</param>
        /// <param name="embeddedResource">Embedded Resource Name</param>
        /// <param name="commandCallback">Parameter lambda expression callback in the form (command) => { }</param>
        /// <param name="validateParameters">If set to <c>true</c> validate parameters.</param>
        /// <typeparam name="T">Data Type</typeparam>
        public static IEnumerable<T> GetObjectsFromEmbeddedResource<T>(this IDbConnection connection, string embeddedResource, Action<IDbCommand> commandCallback = null, bool validateParameters = false) where T : class
        {
            return connection.GetObjectsFromEmbeddedResource<T>(embeddedResource, System.Reflection.Assembly.GetCallingAssembly(), commandCallback, validateParameters);
        }


        /// <summary>
        /// Gets mapped objects from an embedded resource.  An exception will be thrown if the connection is not open.
        /// </summary>
        /// <returns>Enumerable of Mapped Objects</returns>
        /// <param name="connection">IDbConnection</param>
        /// <param name="embeddedResource">Embedded Resource Name</param>
        /// <param name="assembly">Assembly Where Embedded Resource Resides</param>
        /// <param name="commandCallback">Parameter lambda expression callback in the form (command) => { }</param>
        /// <param name="validateParameters">If set to <c>true</c> validate parameters.</param>
        /// <typeparam name="T">Data Type</typeparam>
        public static IEnumerable<T> GetObjectsFromEmbeddedResource<T>(this IDbConnection connection, string embeddedResource, System.Reflection.Assembly assembly, Action<IDbCommand> commandCallback = null, bool validateParameters = false) where T : class
        {
            if (embeddedResource == null)
                throw new ArgumentNullException(nameof(embeddedResource));
            if (assembly == null)
                throw new ArgumentNullException(nameof(assembly));

            IEnumerable<T> result = null;

            connection.ExecuteQueryEmbeddedResource(embeddedResource, assembly, commandCallback, reader =>
            {
                result = reader.MapToObjects<T>();
            }, validateParameters);

            return result;
        }

        #endregion


        #region ExecuteNonQueryStringForObject Methods

        /// <summary>
        /// Executes a Non Query SQL Statement for a DbField decorated object.  Sql Parameters will be automatically populated.
        /// An exception will be thrown if the connection is not open.
        /// </summary>
        /// <param name="connection">IDbConnection</param>
        /// <param name="nonQuerySqlString">Non Query SQL String</param>
        /// <param name="obj">Model object from which parameters are to be populated</param>
        /// <param name="validateParameters">If set to <c>true</c> validate parameters.</param>
        /// <returns>Number of records affected</returns>
        /// <typeparam name="T">Data Type</typeparam>
        public static int ExecuteNonQueryStringForObject<T>(this IDbConnection connection, string nonQuerySqlString, T obj, bool validateParameters = false) where T : class
        {
            if (nonQuerySqlString == null)
                throw new ArgumentNullException(nameof(nonQuerySqlString));
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            return connection.ExecuteNonQueryString(nonQuerySqlString, command => command.BindParametersFromObject(obj), validateParameters);
        }


        /// <summary>
        /// Executes a Non Query SQL Statement from an embedded resource for a DbField decorated object.  Sql Parameters will be automatically populated.
        /// An exception will be thrown if the connection is not open.
        /// </summary>
        /// <param name="connection">IDbConnection</param>
        /// <param name="embeddedResource">Embedded Resource Name</param>
        /// <param name="obj">Model object from which parameters are to be populated</param>
        /// <param name="validateParameters">If set to <c>true</c> validate parameters.</param>
        /// <returns>Number of records affected</returns>
        /// <typeparam name="T">Data Type</typeparam>
        public static int ExecuteNonQueryEmbeddedResourceForObject<T>(this IDbConnection connection, string embeddedResource, T obj, bool validateParameters = false) where T : class
        {
            return ExecuteNonQueryEmbeddedResourceForObject(connection, embeddedResource, System.Reflection.Assembly.GetCallingAssembly(), obj, validateParameters);
        }


        /// <summary>
        /// Executes a Non Query SQL Statement from an embedded resource for a DbField decorated object.  Sql Parameters will be automatically populated.
        /// An exception will be thrown if the connection is not open.
        /// </summary>
        /// <param name="connection">IDbConnection</param>
        /// <param name="embeddedResource">Embedded Resource Name</param>
        /// <param name="assembly">Assembly Where Embedded Resource Resides</param>
        /// <param name="obj">Model object from which parameters are to be populated</param>
        /// <param name="validateParameters">If set to <c>true</c> validate parameters.</param>
        /// <returns>Number of records affected</returns>
        /// <typeparam name="T">Data Type</typeparam>
        public static int ExecuteNonQueryEmbeddedResourceForObject<T>(this IDbConnection connection, string embeddedResource, System.Reflection.Assembly assembly, T obj, bool validateParameters = false) where T : class
        {
            if (embeddedResource == null)
                throw new ArgumentNullException(nameof(embeddedResource));
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));
            if (assembly == null)
                throw new ArgumentNullException(nameof(assembly));

            string sql = assembly.GetEmbeddedResourceString(embeddedResource);

            return connection.ExecuteNonQueryStringForObject(sql, obj, validateParameters);
        }

        #endregion


        #region ExecuteNonQueryForObjects Methods

        /// <summary>
        /// Executes a Non Query SQL Statement for a enumerable list of DbField decorated objects.  Sql Parameters will be automatically populated.
        /// An exception will be thrown if the connection is not open.
        /// </summary>
        /// <param name="connection">IDbConnection</param>
        /// <param name="nonQuerySqlString">Non Query SQL String</param>
        /// <param name="objects">Enumerable list of model objects from which parameters are to be populated</param>
        /// <param name="validateParameters">If set to <c>true</c> validate parameters.</param>
        /// <returns>Number of records affected</returns>
        /// <typeparam name="T">Data Type</typeparam>
        public static void ExecuteNonQueryStringForObjects<T>(this IDbConnection connection, string nonQuerySqlString, IEnumerable<T> objects, bool validateParameters = false) where T : class
        {
            if (nonQuerySqlString == null)
                throw new ArgumentNullException(nameof(nonQuerySqlString));
            if (objects == null)
                throw new ArgumentNullException(nameof(objects));

            foreach (T obj in objects) {
                connection.ExecuteNonQueryString(nonQuerySqlString, command => command.BindParametersFromObject(obj), validateParameters);   
            }
             
        }

        /// <summary>
        /// Executes a Non Query SQL Statement from an embedded resource for a enumerable list of DbField decorated objects.  Sql Parameters will be automatically populated.
        /// An exception will be thrown if the connection is not open.
        /// </summary>
        /// <param name="connection">IDbConnection</param>
        /// <param name="embeddedResource">Embedded Resource Name</param>
        /// <param name="objects">Enumerable list of model objects from which parameters are to be populated</param>
        /// <param name="validateParameters">If set to <c>true</c> validate parameters.</param>
        /// <typeparam name="T">Data Type</typeparam>
        public static void ExecuteNonQueryEmbeddedResourceForObjects<T>(this IDbConnection connection, string embeddedResource, IEnumerable<T> objects, bool validateParameters = false) where T : class
        {
            connection.ExecuteNonQueryEmbeddedResourceForObjects(embeddedResource, System.Reflection.Assembly.GetCallingAssembly(), objects, validateParameters);
        }


        /// <summary>
        /// Executes a Non Query SQL Statement from an embedded resource for a enumerable list of DbField decorated objects.  Sql Parameters will be automatically populated.
        /// An exception will be thrown if the connection is not open.
        /// </summary>
        /// <param name="connection">IDbConnection</param>
        /// <param name="embeddedResource">Embedded Resource Name</param>
        /// <param name="assembly">Assembly Where Embedded Resource Resides</param>
        /// <param name="objects">Enumerable list of model objects from which parameters are to be populated</param>
        /// <param name="validateParameters">If set to <c>true</c> validate parameters.</param>
        /// <typeparam name="T">Data Type</typeparam>
        public static void ExecuteNonQueryEmbeddedResourceForObjects<T>(this IDbConnection connection, string embeddedResource, System.Reflection.Assembly assembly, IEnumerable<T> objects, bool validateParameters = false) where T : class
        {
            if (embeddedResource == null)
                throw new ArgumentNullException(nameof(embeddedResource));
            if (objects == null)
                throw new ArgumentNullException(nameof(objects));
            if (assembly == null)
                throw new ArgumentNullException(nameof(assembly));

            foreach (T obj in objects)
            {
                connection.ExecuteNonQueryEmbeddedResourceForObject(embeddedResource, assembly, obj, validateParameters);
            }
        }

        #endregion
    }
}