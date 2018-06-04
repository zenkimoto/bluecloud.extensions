using BlueCloud.Extensions.Data;
using System;
using Microsoft.Data.Sqlite;
using Xunit;
using System.Data;

namespace BlueCloud.Extensions.Tests
{
    public class IDbCommandExtensionsTests : IDisposable
    {
        SqliteConnection connection;

        public IDbCommandExtensionsTests() 
        {
            connection = new SqliteConnection("Data Source=./Database/chinook.db");
            connection.Open();
        }

        public void Dispose()
        {
            connection.Close();
            connection.Dispose();
        }

        [Fact]
        public void LoadEmbeddedResource_ShouldReadFileSuccessfully() 
        {
            var command = connection.CreateCommand();

            command.LoadEmbeddedResource("GetAllAlbums.sql");

            Assert.Equal("SELECT * FROM albums", command.CommandText);
        }

        [Fact]
        public void ValidateParameters_WhenParametersMatch_ShouldNotThrowException() 
        {
            var command = connection.CreateCommand();

            command.CommandText = "SELECT * FROM albums WHERE AlbumId = @albumid";

            command.AddParameter<int>("albumid", 1);

            command.ValidateParameters();
        }

        [Fact]
        public void ValidateParameters_NoParameters_ShouldThrowException()
        {
            var command = connection.CreateCommand();

            command.CommandText = "SELECT * FROM albums WHERE AlbumId = @albumid";

            Assert.Throws(typeof(DataException), () =>
            {
                command.ValidateParameters();
            });
        }
    }
}
