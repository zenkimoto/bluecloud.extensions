using System;
using Xunit;
using BlueCloud.Extensions.Data;
using Microsoft.Data.Sqlite;
using BlueCloud.Extensions.Tests.Model;
using System.Linq;
using BlueCloud.Extensions.Tests.Database;
using System.Data;
using System.Collections.Generic;

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

            string sql = "SELECT * FROM albums WHERE AlbumId = @albumId";

            connection.ExecuteQueryString(sql, command => command.AddParameter("albumId", 1), reader =>
            {
                reader.Read();

                title = reader.GetValue<string>("Title");
            });

            Assert.Equal("For Those About To Rock We Salute You", title);
        }

        [Fact]
        public void ExecuteQueryString_WhenReaderCallbackIsNull_ShouldThrowException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                connection.ExecuteQueryString("SELECT * FROM albums", null);
            });
        }


        #endregion


        #region ExecuteQueryEmbeddedResource Tests

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
        public void ExecuteQueryEmbeddedResource_WhenEmbeddedResourceIsNull_ShouldThrowException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                connection.ExecuteQueryEmbeddedResource(null, reader => { });
            });
        }

        [Fact]
        public void ExecuteQueryEmbeddedResource_WhenReaderCallbackIsNull_ShouldThrowException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                connection.ExecuteQueryEmbeddedResource("GetAllAlbums.sql", null);
            });
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

        [Fact]
        public void ExecuteNonQuery_ShouldInsertRow()
        {
            string sql = "INSERT INTO albums VALUES (@AlbumId, @Title, @ArtistId)";

            connection.ExecuteNonQueryString(sql, command =>
            {
                command.AddParameter("AlbumId", 348);
                command.AddParameter("Title", "Duo Sonatas 2");
                command.AddParameter("ArtistId", 274);
            });

            connection.ExecuteQueryString("SELECT * FROM albums WHERE AlbumId = 348", reader =>
            {
                reader.Read();

                Assert.Equal("Duo Sonatas 2", reader.GetValue<string>("Title"));
                Assert.Equal(274, reader.GetValue<int>("ArtistId"));
            });
        }

        [Fact]
        public void ExecuteNonQueryString_WhenNullSqlString_ShouldThrowException()
        {
            Assert.Throws<ArgumentNullException>(() => connection.ExecuteNonQueryString(null));
        }

        #endregion


        #region ExecuteNonQueryEmbeddedResource Tests

        [Fact]
        public void ExecuteNonQueryEmbeddedResource_ShouldInsertRow()
        {
            connection.ExecuteNonQueryEmbeddedResource("InsertIntoAlbums.sql", command =>
            {
                command.AddParameter("AlbumId", 348);
                command.AddParameter("Title", "Duo Sonatas 2");
                command.AddParameter("ArtistId", 274);
            });

            connection.ExecuteQueryString("SELECT * FROM albums WHERE AlbumId = 348", reader =>
            {
                reader.Read();

                Assert.Equal("Duo Sonatas 2", reader.GetValue<string>("Title"));
                Assert.Equal(274, reader.GetValue<int>("ArtistId"));
            });
        }

        [Fact]
        public void ExecuteNonQueryEmbeddedResource_WhenNullEmbeddedResource_ShouldThrowException()
        {
            Assert.Throws<ArgumentNullException>(() => connection.ExecuteNonQueryEmbeddedResource(null));
        }

        [Fact]
        public void ExecuteNonQueryEmbeddedResource_WhenNullAssembly_ShouldThrowException()
        {
            Assert.Throws<ArgumentNullException>(() => connection.ExecuteNonQueryEmbeddedResource("InsertIntoAlbums.sql", (System.Reflection.Assembly)null));
        }

        #endregion


        #region ExecuteQueryScalar Tests

        [Fact]
        public void ExecuteQueryScalar_ShouldReturnScalarValue()
        {
            long scalar = (long)connection.ExecuteQueryScalar("SELECT AlbumId FROM albums WHERE AlbumId = 1");

            Assert.Equal(1, scalar);
        }

        [Fact]
        public void ExecuteQueryScalar_WhenNullSqlString_ShouldThrowException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                connection.ExecuteQueryScalar(null);
            });
        }

        #endregion


        #region ExecuteScalarEmbeddedResource Tests

        [Fact]
        public void ExecuteScalarEmbeddedResource_ShouldReturnScalarValue()
        {
            long scalar = (long)connection.ExecuteScalarEmbeddedResource("GetAlbumScalar.sql", System.Reflection.Assembly.GetExecutingAssembly());

            Assert.Equal(1, scalar);
        }

        [Fact]
        public void ExecuteScalarEmbeddedResource_WhenNullEmbeddedResource_ShouldThrowException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                connection.ExecuteScalarEmbeddedResource(null);
            });
        }

        [Fact]
        public void ExecuteScalarEmbeddedResource_WhenNullAssembly_ShouldThrowException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                connection.ExecuteScalarEmbeddedResource("GetAlbumScalar.sql", (System.Reflection.Assembly)null);
            });
        }

        #endregion


        #region GetSingleObjectFromQueryString Tests 

        [Fact]
        public void GetSingleObjectFromQueryString_ShouldReturnSingleObjectFromDatabase()
        {
            var album = connection.GetSingleObjectFromQueryString<Album>("SELECT * FROM albums WHERE AlbumId = 1");

            Assert.Equal(1, album.AlbumId);
            Assert.Equal("For Those About To Rock We Salute You", album.Title);
            Assert.Equal(1, album.ArtistId);
        }

        [Fact]
        public void GetSingleObjectFromQueryString_WhenNullQueryString_ShouldThrowException()
        {
            Assert.Throws<ArgumentNullException>(() => connection.GetSingleObjectFromQueryString<Album>(null));
        }

        [Fact]
        public void GetSingleObjectFromQueryString_WhenMoreThanOneResult_ShouldReturnSingleObjectFromDatabase()
        {
            var album = connection.GetSingleObjectFromQueryString<Album>("SELECT * FROM albums");

            Assert.Equal(1, album.AlbumId);
            Assert.Equal("For Those About To Rock We Salute You", album.Title);
            Assert.Equal(1, album.ArtistId);
        }

        #endregion


        #region GetSingleObjectFromEmbeddedResource Tests

        [Fact]
        public void GetSingleObjectFromEmbeddedResource_GetSingleObjectFromQueryString_ShouldReturnSingleObjectFromDatabase()
        {
            var album = connection.GetSingleObjectFromEmbeddedResource<Album>("GetSingleAlbum.sql", command =>
            {
                command.AddParameter("AlbumId", 1);
            });

            Assert.Equal(1, album.AlbumId);
            Assert.Equal("For Those About To Rock We Salute You", album.Title);
            Assert.Equal(1, album.ArtistId);
        }

        [Fact]
        public void GetSingleObjectFromEmbeddedResource_WhenNullEmbeddedResource_ShouldThrowException()
        {
            Assert.Throws<ArgumentNullException>(() => connection.GetSingleObjectFromEmbeddedResource<Album>(null));
        }

        [Fact]
        public void GetSingleObjectFromEmbeddedResource_WhenNullAssembly_ShouldThrowException()
        {
            Assert.Throws<ArgumentNullException>(() => connection.GetSingleObjectFromEmbeddedResource<Album>("GetSingleAlbum.sql", (System.Reflection.Assembly)null));
        }

        #endregion

        #region GetObjectsFromQueryString Tests

        [Fact]
        public void GetObjectsFromQueryString_ShouldReturnObjectsFromDatabase()
        {
            var albums = connection.GetObjectsFromQueryString<Album>("SELECT * FROM albums WHERE AlbumId IN (1, 2, 3)").ToList();

            Assert.Equal(1, albums[0].AlbumId);
            Assert.Equal("For Those About To Rock We Salute You", albums[0].Title);
            Assert.Equal(1, albums[0].ArtistId);

            Assert.Equal(2, albums[1].AlbumId);
            Assert.Equal("Balls to the Wall", albums[1].Title);
            Assert.Equal(2, albums[1].ArtistId);

            Assert.Equal(3, albums[2].AlbumId);
            Assert.Equal("Restless and Wild", albums[2].Title);
            Assert.Equal(2, albums[2].ArtistId);
        }

        [Fact]
        public void GetObjectsFromQueryString_WhenNullSqlString_ShouldThrowException()
        {
            Assert.Throws<ArgumentNullException>(() => connection.GetObjectsFromQueryString<Album>(null));
        }

        #endregion


        #region GetObjectsFromEmbeddedResource Tests

        [Fact]
        public void GetObjectsFromEmbeddedResource_ShouldReturnObjectsFromDatabase()
        {
            var albums = connection.GetObjectsFromEmbeddedResource<Album>("GetThreeAlbums.sql").ToList();

            Assert.Equal(1, albums[0].AlbumId);
            Assert.Equal("For Those About To Rock We Salute You", albums[0].Title);
            Assert.Equal(1, albums[0].ArtistId);

            Assert.Equal(2, albums[1].AlbumId);
            Assert.Equal("Balls to the Wall", albums[1].Title);
            Assert.Equal(2, albums[1].ArtistId);

            Assert.Equal(3, albums[2].AlbumId);
            Assert.Equal("Restless and Wild", albums[2].Title);
            Assert.Equal(2, albums[2].ArtistId);
        }

        [Fact]
        public void GetObjectsFromEmbeddedResource_WhenNullEmbeddedResource_ShouldThrowException()
        {
            Assert.Throws<ArgumentNullException>(() => connection.GetObjectsFromEmbeddedResource<Album>(null));
        }

        [Fact]
        public void GetObjectsFromEmbeddedResource_WhenNullAssembly_ShouldThrowException()
        {
            Assert.Throws<ArgumentNullException>(() => connection.GetObjectsFromEmbeddedResource<Album>("GetThreeAlbums.sql", (System.Reflection.Assembly)null));
        }

        #endregion


        #region ExecuteNonQueryStringForObject Tests

        [Fact]
        public void ExecuteNonQueryStringForObject_ShouldInsertPropertiesIntoDatabase()
        {
            var album = new Album()
            {
                AlbumId = 348,
                Title = "Duo Sonatas 2",
                ArtistId = 274
            };

            connection.ExecuteNonQueryStringForObject("INSERT INTO albums VALUES (@AlbumId, @Title, @ArtistId)", album);

            connection.ExecuteQueryString("SELECT * FROM albums WHERE AlbumId = 348", reader =>
            {
                reader.Read();

                Assert.Equal("Duo Sonatas 2", reader.GetValue<string>("Title"));
                Assert.Equal(274, reader.GetValue<int>("ArtistId"));
            });
        }

        [Fact]
        public void ExecuteNonQueryStringForObject_WhenNullSqlString_ShouldThrowException()
        {
            var album = new Album()
            {
                AlbumId = 348,
                Title = "Duo Sonatas 2",
                ArtistId = 274
            };

            Assert.Throws<ArgumentNullException>(() => connection.ExecuteNonQueryStringForObject(null, album));
        }

        [Fact]
        public void ExecuteNonQueryStringForObject_WhenNullModel_ShouldThrowException()
        {
            Assert.Throws<ArgumentNullException>(() => connection.ExecuteNonQueryStringForObject("INSERT INTO albums VALUES (@AlbumId, @Title, @ArtistId)", (Album)null));
        }

        #endregion


        #region ExecuteNonQueryEmbeddedResourceForObject Tests

        [Fact]
        public void ExecuteNonQueryEmbeddedResourceForObject_ShouldInsertPropertiesIntoDatabase()
        {
            var album = new Album()
            {
                AlbumId = 348,
                Title = "Duo Sonatas 2",
                ArtistId = 274
            };

            connection.ExecuteNonQueryEmbeddedResourceForObject("InsertIntoAlbums.sql", album);

            connection.ExecuteQueryString("SELECT * FROM albums WHERE AlbumId = 348", reader =>
            {
                reader.Read();

                Assert.Equal("Duo Sonatas 2", reader.GetValue<string>("Title"));
                Assert.Equal(274, reader.GetValue<int>("ArtistId"));
            });
        }

        [Fact]
        public void ExecuteNonQueryEmbeddedResourceForObject_WhenNullEmbeddedResource_ShouldThrowException()
        {
            var album = new Album()
            {
                AlbumId = 348,
                Title = "Duo Sonatas 2",
                ArtistId = 274
            };

            Assert.Throws<ArgumentNullException>(() => connection.ExecuteNonQueryEmbeddedResourceForObject(null, album));
        }

        [Fact]
        public void ExecuteNonQueryEmbeddedResourceForObject_WhenNullModel_ShouldThrowException()
        {
            Assert.Throws<ArgumentNullException>(() => connection.ExecuteNonQueryEmbeddedResourceForObject("InsertIntoAlbums.sql", (Album)null));
        }

        [Fact]
        public void ExecuteNonQueryEmbeddedResourceForObject_WhenNullAssembly_ShouldThrowException()
        {
            var album = new Album()
            {
                AlbumId = 348,
                Title = "Duo Sonatas 2",
                ArtistId = 274
            };

            Assert.Throws<ArgumentNullException>(() => connection.ExecuteNonQueryEmbeddedResourceForObject("InsertIntoAlbums.sql", null, album));
        }

        #endregion

        #region ExecuteNonQueryEmbeddedResourceForObjects Tests

        [Fact]
        public void ExecuteNonQueryEmbeddedResourceForObjects_ShouldInsertRecordsIntoDatabase()
        {
            var albums = new List<Album>()
            {
                new Album()
                {
                    AlbumId = 348,
                    Title = "Duo Sonatas 2",
                    ArtistId = 274
                },
                new Album()
                {
                    AlbumId = 349,
                    Title = "Duo Sonatas 3",
                    ArtistId = 274
                },
                new Album()
                {
                    AlbumId = 350,
                    Title = "Duo Sonatas 4",
                    ArtistId = 274
                },
            };

            connection.ExecuteNonQueryEmbeddedResourceForObjects("InsertIntoAlbums.sql", albums);

            connection.ExecuteQueryString("SELECT * FROM albums WHERE AlbumId IN (348, 349, 350)", reader =>
            {
                reader.Read();

                Assert.Equal(348, reader.GetValue<int>("AlbumId"));
                Assert.Equal("Duo Sonatas 2", reader.GetValue<string>("Title"));

                reader.Read();

                Assert.Equal(349, reader.GetValue<int>("AlbumId"));
                Assert.Equal("Duo Sonatas 3", reader.GetValue<string>("Title"));

                reader.Read();

                Assert.Equal(350, reader.GetValue<int>("AlbumId"));
                Assert.Equal("Duo Sonatas 4", reader.GetValue<string>("Title"));
            });
        }

        [Fact]
        public void ExecuteNonQueryEmbeddedResourceForObjects_WhenNullEmbeddedResource_ShouldThrowException()
        {
            Assert.Throws<ArgumentNullException>(() => connection.ExecuteNonQueryEmbeddedResourceForObjects(null, new List<Album>()));
        }

        [Fact]
        public void ExecuteNonQueryEmbeddedResourceForObjects_WhenNullModel_ShouldThrowException()
        {
            Assert.Throws<ArgumentNullException>(() => connection.ExecuteNonQueryEmbeddedResourceForObjects("InsertIntoAlbums.sql", (List<Album>)null));
        }

        [Fact]
        public void ExecuteNonQueryEmbeddedResourceForObjects_WhenNullAssembly_ShouldThrowException()
        {
            Assert.Throws<ArgumentNullException>(() => connection.ExecuteNonQueryEmbeddedResourceForObjects("InsertIntoAlbums.sql", null, new List<Album>()));
        }

        #endregion

        #region IDbMappable Tests

        [Fact]
        public void TestBooleanDatabaseConversion_ShouldConvertCorrectly() {
            var booleanTest = new BooleanTest();

            connection.ExecuteNonQueryString("CREATE TABLE BOOLEAN_TEST (boolean_value NUMBER, long_value NUMBER)");

            connection.ExecuteNonQueryStringForObject<BooleanTest>("INSERT INTO BOOLEAN_TEST (boolean_value, long_value) VALUES (@boolean_value, @long_value)", booleanTest);

            var actualTest = connection.GetSingleObjectFromQueryString<BooleanTest>("SELECT * FROM BOOLEAN_TEST");

            Assert.Equal(booleanTest.BooleanValue, actualTest.BooleanValue);
            Assert.True(actualTest.BooleanValue);
            Assert.Equal(123, actualTest.LongValue);
        }

        #endregion
    }
}
