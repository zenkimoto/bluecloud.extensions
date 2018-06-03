using System;
using Xunit;
using BlueCloud.Extensions.Data;
using Microsoft.Data.Sqlite;
using BlueCloud.Extensions.Tests.Model;
using System.Linq;
using System.Data;
using System.Diagnostics;

namespace BlueCloud.Extensions.Tests
{
    public class IDataReaderExtensionsTests : IDisposable
    {
        SqliteConnection connection;
        SqliteCommand command;
        IDataReader reader;

        public IDataReaderExtensionsTests()
        {
            connection = new SqliteConnection("Data Source=./Database/chinook.db");
            connection.Open();

            command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM albums";

            reader = command.ExecuteReader();
        }

        public void Dispose()
        {
            reader.Close();
            reader.Dispose();

            command.Dispose();

            connection.Close();
            connection.Dispose();
            connection = null;
        }

        [Fact]
        public void GetValue_ShouldReturnCorrectValue()
        {
            reader.Read();

            var albumId = reader.GetValue<int>("AlbumId");
            var title = reader.GetValue<string>("Title");
            var artistId = reader.GetValue<int>("ArtistId");

            Assert.Equal(1, albumId);
            Assert.Equal("For Those About To Rock We Salute You", title);
            Assert.Equal(1, artistId);
        }

        [Fact]
        public void BaseGetValueBenchark()
        {
            reader.Read();

            var time = MeasurePerformance(() =>
            {
                for (int i = 0; i < 10000; i++) {
                    reader.GetInt32(reader.GetOrdinal("AlbumId"));
                    reader.GetString(reader.GetOrdinal("Title"));
                    reader.GetInt32(reader.GetOrdinal("ArtistId"));
                }
            });

            Assert.True(time < 1, $"Base Benchmark Get Value: {time}");
        }

        [Fact]
        public void GetValue_Benchmark()
        {
            reader.Read();

            var time = MeasurePerformance(() =>
            {
                for (int i = 0; i < 10000; i++)
                {
                    reader.GetValue<int>("AlbumId");
                    reader.GetValue<string>("Title");
                    reader.GetValue<int>("ArtistId");
                }
            });

            Assert.True(time < 1, $"Base Benchmark Get Value: {time}");
        }

        private long MeasurePerformance(Action action)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            action.Invoke();
            stopwatch.Stop();
            return stopwatch.ElapsedMilliseconds;
        }

        /*
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
        */
    }
}
