using System;
using Xunit;
using BlueCloud.Extensions.Data;
using Microsoft.Data.Sqlite;
using BlueCloud.Extensions.Tests.Model;
using System.Data;
using System.Diagnostics;

namespace BlueCloud.Extensions.Tests.Performance
{
    public class IDataReaderExtensionsPerformanceTests: IDisposable
    {
        SqliteConnection connection;
        SqliteCommand command;
        IDataReader reader;

        public IDataReaderExtensionsPerformanceTests()
        {
            connection = new SqliteConnection("Data Source=:memory:");
            connection.Open();

            using (command = connection.CreateCommand()) {
                command.CommandText = "CREATE TABLE albums ([AlbumId] INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL, [Title] NVARCHAR(160)  NOT NULL, [ArtistId] INTEGER  NOT NULL)";

                command.ExecuteNonQuery();

                command.CommandText = "INSERT INTO albums VALUES (1, 'For Those About To Rock We Salute You', 1)";

                command.ExecuteNonQuery();
            }

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
        public void GetValue_ShouldBePerformant() {
            reader.Read();

            var baseTime = MeasurePerformance(() =>
            {
                for (int i = 0; i < 10000; i++)
                {
                    reader.GetInt32(reader.GetOrdinal("AlbumId"));
                    reader.GetString(reader.GetOrdinal("Title"));
                    reader.GetInt32(reader.GetOrdinal("ArtistId"));
                }
            });

            var actualTime = MeasurePerformance(() =>
            {
                for (int i = 0; i < 10000; i++)
                {
                    reader.GetValue<int>("AlbumId");
                    reader.GetValue<string>("Title");
                    reader.GetValue<int>("ArtistId");
                }
            });

            Assert.True(actualTime <= baseTime * 2.0, $"Actual Time: {actualTime} should be less than 2.0 times {baseTime}");
        }

        [Fact]
        public void MapToObjects_ShouldBePerformant()
        {
            reader.Read();

            var baseTime = MeasurePerformance(() =>
            {
                for (int i = 0; i < 10000; i++)
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

            var actualTime = MeasurePerformance(() =>
            {
                for (int i = 0; i < 10000; i++)
                {
                    reader.MapToObjects<Album>();
                    reader.Close();

                    reader = command.ExecuteReader();
                }
            });

            Assert.True(actualTime <= baseTime * 2.5, $"Actual Time: {actualTime} should be less than 2.5 times {baseTime}");
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
