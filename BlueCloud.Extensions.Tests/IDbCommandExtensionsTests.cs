using BlueCloud.Extensions.Data;
using System;
using Microsoft.Data.Sqlite;
using Xunit;
using System.Data;
using System.IO;
using BlueCloud.Extensions.Tests.Model;

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


        #region LoadEmbeddedResource Tests

        [Fact]
        public void LoadEmbeddedResource_ShouldReadFileSuccessfully() 
        {
            var command = connection.CreateCommand();

            command.LoadEmbeddedResource("GetAllAlbums.sql");

            Assert.Equal("SELECT * FROM albums", command.CommandText);
        }

        [Fact]
        public void LoadEmbeddedResource_WhenFileDoesntExist_ShouldThrowException()
        {
            var command = connection.CreateCommand();

            Assert.Throws<FileNotFoundException>(() =>
            {
                command.LoadEmbeddedResource("GetAllAlbums2.sql");
            });
        }

        #endregion


        #region ValidateParameters Tests

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

            Assert.Throws<DataException>(() =>
            {
                command.ValidateParameters();
            });
        }

        [Fact]
        public void ValidateParameters_WhenTooManyParametersInSql_ShouldThrowException()
        {
            var command = connection.CreateCommand();

            command.CommandText = "SELECT * FROM albums WHERE AlbumId = @albumid AND ArtistId = @artistid";

            Assert.Throws<DataException>(() =>
            {
                command.ValidateParameters();
            });
        }

        #endregion


        #region ParameterNamesFromCommandText Tests

        [Fact]
        public void ParameterNamesFromCommandText_ShouldReturnAllParametersInSQL()
        {
            var command = connection.CreateCommand();

            command.CommandText = "SELECT * FROM albums WHERE AlbumId = @albumid AND ArtistId = @artistid";

            var parameters = command.ParameterNamesFromCommandText();

            Assert.True(parameters.Contains("@albumid"), "Parameter list should contain 'albumid'");
            Assert.True(parameters.Contains("@artistid"), "Parameter list should contain 'artistid'");
            Assert.Equal(2, parameters.Count);
        }

        [Fact]
        public void ParameterNamesFromCommandText_WhenSpacesAreRemovedAndParenthesisAdded_ShouldReturnAllParametersInSQL()
        {
            var command = connection.CreateCommand();

            command.CommandText = "SELECT * FROM albums WHERE (AlbumId=@albumid) AND (ArtistId=@artistid)";

            var parameters = command.ParameterNamesFromCommandText();

            Assert.True(parameters.Contains("@albumid"), "Parameter list should contain 'albumid'");
            Assert.True(parameters.Contains("@artistid"), "Parameter list should contain 'artistid'");
            Assert.Equal(2, parameters.Count);
        }

        [Fact]
        public void ParameterNamesFromCommandText_WhenNoParametersSpecified_ShouldReturnEmptyArray()
        {
            var command = connection.CreateCommand();

            command.CommandText = "SELECT * FROM albums WHERE AlbumId = 1";

            var parameters = command.ParameterNamesFromCommandText();

            Assert.Empty(parameters);
        }

        #endregion


        #region BindParametersFromObject Tests

        [Fact]
        public void BindParametersFromObject_ShouldAddParametersFromProjectProperties()
        {
            var command = connection.CreateCommand();

            var album = new Album
            {
                AlbumId = 100,
                Title = "Test Album",
                ArtistId = 101
            };

            command.CommandText = "INSERT INTO albums (AlbumId, Title, ArtistId) VALUES (@AlbumId, @Title, @ArtistId)";

            command.BindParametersFromObject(album);

            Assert.Equal((long)100, command.Parameters["@AlbumId"].Value);
            Assert.Equal("Test Album", command.Parameters["@Title"].Value);
            Assert.Equal((long)101, command.Parameters["@ArtistId"].Value);
            Assert.Equal(3, command.Parameters.Count);
        }

        [Fact]
        public void BindParametersFromObject_WhenParametersDontExistInSQL_ShouldNotAddAnyParameters()
        {
            var command = connection.CreateCommand();

            var album = new Album
            {
                AlbumId = 100,
                Title = "Test Album",
                ArtistId = 101
            };

            command.CommandText = "INSERT INTO albums (AlbumId, Title, ArtistId) VALUES (100, 'Test Album', 102)";

            command.BindParametersFromObject(album);

            Assert.Equal(0, command.Parameters.Count);
        }

        [Fact]
        public void BindParametersFromObject_WhenPartialParametersExistInSQL_ShouldOnlyParatialParameters()
        {
            var command = connection.CreateCommand();

            var album = new Album
            {
                AlbumId = 100,
                Title = "Test Album",
                ArtistId = 101
            };

            command.CommandText = "INSERT INTO albums (AlbumId, Title, ArtistId) VALUES (@AlbumId, 'Test Album', 102)";

            command.BindParametersFromObject(album);

            Assert.Equal((long)100, command.Parameters["@AlbumId"].Value);
            Assert.Equal(1, command.Parameters.Count);
        }

        #endregion

    }
}
