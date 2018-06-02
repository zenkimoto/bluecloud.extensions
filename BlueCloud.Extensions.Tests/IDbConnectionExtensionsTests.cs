using System;
using Xunit;
using BlueCloud.Extensions.Data;
using Microsoft.Data.Sqlite;
using BlueCloud.Extensions.Tests.Model;
using System.Linq;

namespace BlueCloud.Extensions.Tests
{
    public class IDbConnectionExtensionsTests: IDisposable
    {
        SqliteConnection connection;

        public IDbConnectionExtensionsTests() 
        {
            connection = new SqliteConnection("Data Source=./Database/chinook.db");
            connection.Open();
        }

        public void Dispose()
        {
            connection.Close();
            connection.Dispose();
            connection = null;
        }

        [Fact]
        public void ExecuteQueryString_ShouldReturnRows()
        {
            var count = 0;

            connection.ExecuteQueryString("SELECT * FROM albums", reader => {
                while (reader.Read()) {
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

        [Fact]
        public void GetObjectsFromEmbeddedResource_ShouldReturnAlbumObjects()
        {
            var albums = connection.GetObjectsFromEmbeddedResource<Album>("GetAllAlbums.sql");

            Assert.Equal(347, albums.ToList().Count);
        }
    }
}
