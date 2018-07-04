using System;
using Xunit;
using BlueCloud.Extensions.Data;
using Microsoft.Data.Sqlite;
using BlueCloud.Extensions.Tests.Model;
using System.Linq;
using BlueCloud.Extensions.Tests.Database;
using System.Data;

namespace BlueCloud.Extensions.Tests
{
    public class IDbConnectionExtensionsTests : IDisposable
    {
        SqliteConnection connection;

        public IDbConnectionExtensionsTests()
        {
            connection = new SqliteConnection("Data Source=:memory:");
            connection.Open();

            InMemoryDatabase.Setup(connection);
        }

        public void Dispose()
        {
            connection.Close();
            connection.Dispose();
            connection = null;
        }

        #region CommandWithSqlString Tests

        [Fact]
        public void CommandWithSqlString_ShouldCreateCommandWithSQL()
        {
            var command = connection.CommandWithSqlString("SELECT * FROM invoice");

            Assert.Equal("SELECT * FROM invoice", command.CommandText);
            Assert.Equal(CommandType.Text, command.CommandType);
        }

        [Fact]
        public void CommandWithSqlString_WhenNullSqlString_ShouldThrowArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                connection.CommandWithSqlString(null);
            });
        }

        #endregion


        #region CommandWithStoredProcedure Tests

        // TODO: Stored Procedures not supported with Sqlite

        /*
        [Fact]
        public void CommandWithStoredProcedure_ShouldCreateCommand() 
        {
            var command = connection.CommandWithStoredProcedure("storedProcedure()");

            Assert.Equal("storedProcedure()", command.CommandText);
            Assert.Equal(CommandType.StoredProcedure, command.CommandType);
        }

        [Fact]
        public void CommandWithStoredProcedure_WhenNullSqlString_ShouldThrowArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                connection.CommandWithStoredProcedure(null);
            });
        }
        */

        #endregion


        #region CommandWithEmbeddedResource Tests

        [Fact]
        public void CommandWithEmbeddedResource_ShouldCreateCommandFromEmbeddedResource()
        {
            var command = connection.CommandWithEmbeddedResource("GetAllAlbums.sql");

            Assert.Equal("SELECT * FROM albums", command.CommandText);
            Assert.Equal(CommandType.Text, command.CommandType);
        }

        [Fact]
        public void CommandWithEmbeddedResource_WhenNullEmbeddedResource_ShouldThrowException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                connection.CommandWithEmbeddedResource(null);
            });
        }

        [Fact]
        public void CommandWithEmbeddedResource_WhenNullAssembly_ShouldThrowException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                connection.CommandWithEmbeddedResource("GetAllAlbums.sql", null);
            });
        }

        #endregion


        #region ExecuteQueryString Tests

        [Fact]
        public void ExecuteQueryString_ShouldReturnRows()
        {
            var count = 0;

            connection.ExecuteQueryString("SELECT * FROM albums", reader =>
            {
                while (reader.Read())
                {
                    count++;
                }
            });

            Assert.Equal(347, count);
        }

        [Fact]
        public void ExecuteQueryString_WhenAddingParameter_ShouldReturnCorrectRow()
        {
            var title = "";

            connection.ExecuteQueryString("SELECT * FROM albums WHERE AlbumId = @albumId", command =>
            {
                command.AddParameter<int>("albumId", 1);
            }, reader =>
            {
                reader.Read();
                title = reader.GetValue<string>("Title");
            });

            Assert.Equal("For Those About To Rock We Salute You", title);
        }

        [Fact]
        public void ExecuteQueryEmbeddedResource_ShouldReturnRows()
        {
            var count = 0;

            connection.ExecuteQueryEmbeddedResource("GetAllAlbums.sql", reader =>
            {
                while (reader.Read())
                {
                    count++;
                }
            });

            Assert.Equal(347, count);
        }

        #endregion


        #region GetObjectsFromEmbeddedResource Tests

        [Fact]
        public void GetObjectsFromEmbeddedResource_ShouldReturnAlbumObjects()
        {
            var albums = connection.GetObjectsFromEmbeddedResource<Album>("GetAllAlbums.sql");

            Assert.Equal(347, albums.ToList().Count);
        }

        #endregion


        #region ExecuteNonQuery Tests

        [Fact]
        public void ExecuteNonQuery_ShouldExecuteDML()
        {
            connection.ExecuteNonQueryString("CREATE TABLE test_table (id VARCHAR(20))");

            connection.ExecuteQueryString("SELECT name FROM sqlite_master WHERE type='table' AND name='test_table'", reader =>
            {
                reader.Read();

                Assert.Equal("test_table", reader[0]);
            });
        }

        #endregion
    }
}
