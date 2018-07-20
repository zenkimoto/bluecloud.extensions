using Microsoft.Data.Sqlite;
using System;
using System.Data;

namespace Sample2.DataAccess
{
    public abstract class BaseDao
    {
        private readonly string connectionString;

        public BaseDao(string connectionString)
        {
            this.connectionString = connectionString;
        }

        /// <summary>
        /// Opens and returns a <see cref="IDbConnection"/>  in a callback function of type T.
        /// </summary>
        /// <typeparam name="T">The <see cref="IDbConnection"/> from type T for which to open a database connection</typeparam>
        /// <param name="callback">Object to call back</param>
        /// <returns>T</returns>
        protected T OpenDatabaseConnection<T>(Func<IDbConnection, T> callback)
        {
            using (IDbConnection connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                T value = callback(connection);

                connection.Close();

                return value;
            }
        }

        /// <summary>
        /// Opens and returns a <see cref="IDbConnection"/> in a callback function.
        /// </summary>
        /// <param name="callback">The <see cref="IDbConnection"/> for which to open a database connection</param>
        protected void OpenDatabaseConnection(Action<IDbConnection> callback)
        {
            using (IDbConnection connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                callback(connection);

                connection.Close();
            }
        }
    }
}
