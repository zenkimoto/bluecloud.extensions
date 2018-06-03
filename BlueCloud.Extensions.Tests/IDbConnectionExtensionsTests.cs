using System;
using Xunit;
using BlueCloud.Extensions.Data;
using Microsoft.Data.Sqlite;
using BlueCloud.Extensions.Tests.Model;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;

namespace BlueCloud.Extensions.Tests
{
    public class IDbConnectionExtensionsTests : IDisposable
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

        [Fact]
        public void GetObjectsFromEmbeddedResource_ShouldReturnAlbumObjects()
        {
            var albums = connection.GetObjectsFromEmbeddedResource<Album>("GetAllAlbums.sql");

            Assert.Equal(347, albums.ToList().Count);
        }

        [Fact]
        public void GetObjectsFromEmbeddedResource_ShouldBePerformant()
        {
            var elapsedTime = MeasurePerformance(() =>
            {
                for (int i = 0; i < 100; i++) {
                    connection.GetObjectsFromEmbeddedResource<Album>("GetAllAlbums.sql", null, false);    
                }
            });

            Assert.True(elapsedTime < 1, $"Actual Elapsed Time: {elapsedTime} milliseconds");
        }

        [Fact]
        public void GetObjectsFromQueryString_ShouldBePerformant()
        {
            var elapsedTime = MeasurePerformance(() =>
            {
                for (int i = 0; i < 100; i++)
                {
                    connection.GetObjectsFromQueryString<Album>("SELECT * FROM albums", null, false);
                }
            });

            Assert.True(elapsedTime < 1, $"Actual Elapsed Time: {elapsedTime} milliseconds");
        }

        [Fact]
        public void ExecuteQueryForObjects_ShouldBePerformant()
        {
            var elapsedTime = MeasurePerformance(() =>
            {
                for (int i = 0; i < 100; i++)
                {
                    var list = new List<Album>();

                    connection.ExecuteQueryForObjects<Album>("SELECT * FROM albums", reader => {
                        return new Album()
                        {
                            AlbumId = reader.GetInt32(reader.GetOrdinal("AlbumId")),
                            Title = reader.GetString(reader.GetOrdinal("Title")),
                            ArtistId = reader.GetInt32(reader.GetOrdinal("ArtistId"))
                        };
                    });
                }
            });

            Assert.True(elapsedTime < 1, $"Actual Elapsed Time: {elapsedTime} milliseconds");
        }

        [Fact]
        public void ExecuteQueryForObjects2_ShouldBePerformant()
        {
            var elapsedTime = MeasurePerformance(() =>
            {
                for (int i = 0; i < 100; i++)
                {
                    var list = new List<Album>();

                    connection.ExecuteQueryForObjects<Album>("SELECT * FROM albums", reader => {
                        return new Album()
                        {
                            AlbumId = reader.GetValue<int>("AlbumId"),
                            Title = reader.GetValue<string>("Title"),
                            ArtistId = reader.GetValue<int>("ArtistId")
                        };
                    });
                }
            });

            Assert.True(elapsedTime < 1, $"Actual Elapsed Time: {elapsedTime} milliseconds");
        }

        [Fact]
        public void BaseBenchmarkTest()
        {
            var elapsedTime = MeasurePerformance(() =>
            {
                for (int i = 0; i < 100; i++)
                {
                    var list = new List<Album>();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT * FROM albums";

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var album = new Album()
                                {
                                    AlbumId = reader.GetInt32(reader.GetOrdinal("AlbumId")),
                                    Title = reader.GetString(reader.GetOrdinal("Title")),
                                    ArtistId = reader.GetInt32(reader.GetOrdinal("ArtistId"))
                                };

                                list.Add(album);
                            }
                        }
                    }
                }
            });

            Assert.True(elapsedTime < 1, $"Actual Elapsed Time: {elapsedTime} milliseconds");
        }

        private long MeasurePerformance(Action action) {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            action.Invoke();
            stopwatch.Stop();
            return stopwatch.ElapsedMilliseconds;
        }
    }
}
