using System;
using Xunit;
using BlueCloud.Extensions.Data;
using Microsoft.Data.Sqlite;
using BlueCloud.Extensions.Tests.Model;
using System.Data;
using System.Diagnostics;

namespace BlueCloud.Extensions.Tests.Performance
{
    public class IDataReaderExtensionsPerformanceTests
    {
        SqliteConnection connection;
        SqliteCommand command;
        IDataReader reader;

        public IDataReaderExtensionsPerformanceTests()
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
        public void BaseGetValueBenchark()
        {
            reader.Read();

            var time = MeasurePerformance(() =>
            {
                for (int i = 0; i < 10000; i++)
                {
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

        [Fact]
        public void PopulateProperties_ShouldBePerformant()
        {
            reader.Read();

            var time = MeasurePerformance(() =>
            {
                for (int i = 0; i < 10000; i++)
                {
                    var album = new Album();
                    reader.PopulateProperties<Album>(album);
                }
            });

            Assert.True(time < 1, $"Populate Properties benchmark: {time}");
        }

        [Fact]
        public void PopulateProperties_BaseBenchmark()
        {
            reader.Read();

            var time = MeasurePerformance(() =>
            {
                for (int i = 0; i < 10000; i++)
                {
                    var album = new Album();
                    album.AlbumId = reader.GetInt32(reader.GetOrdinal("AlbumId"));
                    album.Title = reader.GetString(reader.GetOrdinal("Title"));
                    album.ArtistId = reader.GetInt32(reader.GetOrdinal("ArtistId"));
                }
            });

            Assert.True(time < 1, $"Base Benchmark Populate Properties: {time}");
        }

        [Fact]
        public void MapToObjects_ShouldBePerformant()
        {
            reader.Read();

            var time = MeasurePerformance(() =>
            {
                for (int i = 0; i < 100; i++)
                {
                    reader.MapToObjects<Album>();
                    reader.Close();

                    reader = command.ExecuteReader();
                }
            });

            Assert.True(time < 1, $"New Benchmark Populate Properties: {time}");
        }

        [Fact]
        public void MapToObjects_BaseBenchmark()
        {
            reader.Read();

            var time = MeasurePerformance(() =>
            {
                for (int i = 0; i < 100; i++)
                {
                    while (reader.Read())
                    {
                        reader.GetInt32(reader.GetOrdinal("AlbumId"));
                        reader.GetString(reader.GetOrdinal("Title"));
                        reader.GetInt32(reader.GetOrdinal("ArtistId"));
                    }

                    reader.Close();
                    reader = command.ExecuteReader();
                }
            });

            Assert.True(time < 1, $"New Base Benchmark Populate Properties: {time}");
        }

        private long MeasurePerformance(Action action)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            action.Invoke();
            stopwatch.Stop();
            return stopwatch.ElapsedMilliseconds;
        }
    }
}
