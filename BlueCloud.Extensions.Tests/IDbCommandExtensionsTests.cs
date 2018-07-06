using BlueCloud.Extensions.Data;
using System;
using Microsoft.Data.Sqlite;
using Xunit;
using System.Data;
using System.IO;
using BlueCloud.Extensions.Tests.Model;
using BlueCloud.Extensions.Tests.Database;
using System.Linq;

namespace BlueCloud.Extensions.Tests
{
    public class IDbCommandExtensionsTests : IDisposable
    {
        SqliteConnection connection;
        SqliteCommand command;

        public IDbCommandExtensionsTests()
        {
            connection = new SqliteConnection("Data Source=:memory:");
            connection.Open();

            InMemoryDatabase.Setup(connection);

            command = connection.CreateCommand();
        }

        public void Dispose()
        {
            connection.Close();
            connection.Dispose();
            connection = null;

            command.Dispose();
            command = null;
        }


        #region Helper Functions

        private Album BuildAlbumItem()
        {
            return new Album
            {
                AlbumId = 100,
                Title = "Test Album",
                ArtistId = 101
            };
        }

        private Invoice BuildInvoiceItem()
        {
            return new Invoice
            {
                InvoiceId = 998,
                CustomerId = 999,
                InvoiceDate = new DateTime(2018, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            };
        }

        private string GetInvoiceInsertSql()
        {
            return "INSERT INTO invoice (InvoiceId, CustomerId, InvoiceDate) VALUES (@InvoiceId, @CustomerId, @InvoiceDate)";
        }

        #endregion


        #region LoadEmbeddedResource Tests

        [Fact]
        public void LoadEmbeddedResource_ShouldReadFileSuccessfully()
        {
            command.LoadEmbeddedResource("GetAllAlbums.sql");

            Assert.Equal("SELECT * FROM albums", command.CommandText);
            Assert.Equal(CommandType.Text, command.CommandType);
        }

        [Fact]
        public void LoadEmbeddedResource_WhenFileDoesntExist_ShouldThrowException()
        {
            Assert.Throws<FileNotFoundException>(() =>
            {
                command.LoadEmbeddedResource("GetAllAlbums2.sql");
            });
        }

        [Fact]
        public void LoadEmbeddedResource_WhenNullEmbeddedResource_ShouldThrowArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                command.LoadEmbeddedResource(null);
            });
        }

        [Fact]
        public void LoadEmbeddedResource_WhenNullAssembly_ShouldThrowArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                command.LoadEmbeddedResource("GetAllAlbums.sql", null);
            });
        }

        #endregion


        #region ValidateParameters Tests

        [Fact]
        public void ValidateParameters_WhenParametersMatch_ShouldNotThrowException()
        {
            command.CommandText = "SELECT * FROM albums WHERE AlbumId = @albumid";

            command.AddParameter("albumid", 1);

            command.ValidateParameters();
        }

        [Fact]
        public void ValidateParameters_NoParameters_ShouldThrowException()
        {
            command.CommandText = "SELECT * FROM albums WHERE AlbumId = @albumid";

            Assert.Throws<DataException>(() =>
            {
                command.ValidateParameters();
            });
        }

        [Fact]
        public void ValidateParameters_WhenTooManyParametersInSql_ShouldThrowException()
        {
            command.CommandText = "SELECT * FROM albums WHERE AlbumId = @albumid AND ArtistId = @artistid";

            Assert.Throws<DataException>(() =>
            {
                command.ValidateParameters();
            });
        }

        #endregion


        #region AddParameter Tests

        [Fact]
        public void AddParameter_ShouldAddParameterToCommandParameters()
        {
            command.AddParameter("testParam", 1);

            Assert.Equal(1, command.Parameters["testParam"].Value);
        }

        [Fact]
        public void AddParameter_WhenPassingInParameterCallback_ShouldAllowCustomization()
        {
            command.AddParameter("testParam", 1, (parameter) =>
            {
                parameter.DbType = DbType.Double;
            });

            Assert.Equal(DbType.Double, command.Parameters["testParam"].DbType);
        }

        [Fact]
        public void AddParameter_WhenNullParameterName_ShouldThrowArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                command.AddParameter(null, 1);
            });
        }

        #endregion


        #region AddOutputParameterTests

        // TODO: Output parameters do not work with Sqlite
        /*
        [Fact]
        public void AddOutputParameter_ShouldAddParameterToCommandParameters()
        {
            command.AddOutputParameter("testParam", DbType.Double);

            Assert.Equal("testParam", command.Parameters[0].ParameterName);
            Assert.Equal(DbType.Double, command.Parameters[0].DbType);
            Assert.Equal(ParameterDirection.Output, command.Parameters[0].Direction);
        }

        [Fact]
        public void AddOutputParameter_WhenNullParameterName_ShouldThrowArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                command.AddOutputParameter(null, DbType.Double);
            });
        }
        */
        #endregion


        #region ParameterNamesFromCommandText Tests

        [Fact]
        public void ParameterNamesFromCommandText_ShouldReturnAllParametersInSQL()
        {
            command.CommandText = "SELECT * FROM albums WHERE AlbumId = @albumid AND ArtistId = @artistid";

            var parameters = command.ParameterNamesFromCommandText().ToList();

            Assert.True(parameters.Contains("@albumid"), "Parameter list should contain 'albumid'");
            Assert.True(parameters.Contains("@artistid"), "Parameter list should contain 'artistid'");
            Assert.Equal(2, parameters.Count);
        }

        [Fact]
        public void ParameterNamesFromCommandText_WhenSpacesAreRemovedAndParenthesisAdded_ShouldReturnAllParametersInSQL()
        {
            command.CommandText = "SELECT * FROM albums WHERE (AlbumId=@albumid) AND (ArtistId=@artistid)";

            var parameters = command.ParameterNamesFromCommandText().ToList();

            Assert.True(parameters.Contains("@albumid"), "Parameter list should contain 'albumid'");
            Assert.True(parameters.Contains("@artistid"), "Parameter list should contain 'artistid'");
            Assert.Equal(2, parameters.Count);
        }

        [Fact]
        public void ParameterNamesFromCommandText_WhenNoParametersSpecified_ShouldReturnEmptyArray()
        {
            command.CommandText = "SELECT * FROM albums WHERE AlbumId = 1";

            var parameters = command.ParameterNamesFromCommandText();

            Assert.Empty(parameters);
        }

        #endregion


        #region BindParametersFromObject Tests

        [Fact]
        public void BindParametersFromObject_ShouldAddParametersFromProjectProperties()
        {
            var album = BuildAlbumItem();

            command.CommandText = "INSERT INTO albums (AlbumId, Title, ArtistId) VALUES (@AlbumId, @Title, @ArtistId)";

            command.BindParametersFromObject(album);

            Assert.Equal((long)100, command.Parameters["AlbumId"].Value);
            Assert.Equal("Test Album", command.Parameters["Title"].Value);
            Assert.Equal((long)101, command.Parameters["ArtistId"].Value);
            Assert.Equal(3, command.Parameters.Count);
        }

        [Fact]
        public void BindParametersFromObject_WhenParametersDontExistInSQL_ShouldNotAddAnyParameters()
        {
            var album = BuildAlbumItem();

            command.CommandText = "INSERT INTO albums (AlbumId, Title, ArtistId) VALUES (100, 'Test Album', 102)";

            command.BindParametersFromObject(album);

            Assert.Equal(0, command.Parameters.Count);
        }

        [Fact]
        public void BindParametersFromObject_WhenPartialParametersExistInSQL_ShouldOnlyParatialParameters()
        {
            var album = BuildAlbumItem();

            command.CommandText = "INSERT INTO albums (AlbumId, Title, ArtistId) VALUES (@AlbumId, 'Test Album', 102)";

            command.BindParametersFromObject(album);

            Assert.Equal((long)100, command.Parameters["AlbumId"].Value);
            Assert.Equal(1, command.Parameters.Count);
        }

        [Fact]
        public void BindParametersFromObject_WhenUsingCustomParameterMapping_ShouldPopulateParametersCorrectly()
        {
            var invoice = BuildInvoiceItem();

            command.CommandText = GetInvoiceInsertSql();

            command.BindParametersFromObject(invoice);

            Assert.Equal((long)1998, command.Parameters["InvoiceId"].Value);
            Assert.Equal((long)1999, command.Parameters["CustomerId"].Value);
            Assert.Equal(new DateTime(2018, 2, 1, 0, 0, 0, DateTimeKind.Utc), command.Parameters["InvoiceDate"].Value);
            Assert.Equal(3, command.Parameters.Count);
        }

        [Fact]
        public void BindParametersFromObject_WhenPassingInANull_ShouldThrowArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                command.BindParametersFromObject<Invoice>(null);
            });
        }

        #endregion


        #region ParameterNames Tests

        [Fact]
        public void ParameterNames_ShouldReturnListOfParameterNames()
        {
            var invoice = BuildInvoiceItem();

            command.CommandText = GetInvoiceInsertSql();

            command.BindParametersFromObject(invoice);

            var names = command.ParameterNames().ToList();

            Assert.Equal(3, names.Count);
            Assert.Equal("InvoiceId", names[0]);
            Assert.Equal("CustomerId", names[1]);
            Assert.Equal("InvoiceDate", names[2]);
        }

        #endregion


        #region RemoveParameter Tests

        [Fact]
        public void RemoveParameter_ShouldRemoveCorrectCommandParameter()
        {
            var invoice = BuildInvoiceItem();

            command.CommandText = GetInvoiceInsertSql();

            command.BindParametersFromObject(invoice);

            command.RemoveParameter("CustomerId");

            Assert.Equal(2, command.Parameters.Count);
            Assert.Equal("InvoiceId", command.Parameters[0].ParameterName);
            Assert.Equal("InvoiceDate", command.Parameters[1].ParameterName);
        }

        [Fact]
        public void RemoveParameter_WhenParameterDoesNotExist_ShouldNotRemoveAny()
        {
            var invoice = BuildInvoiceItem();

            command.CommandText = GetInvoiceInsertSql();

            command.BindParametersFromObject(invoice);

            command.RemoveParameter("RandomParameter");

            Assert.Equal(3, command.Parameters.Count);
        }

        [Fact]
        public void RemoveParameter_WhenParameterNameIsNull_ShouldThrowArgumentException()
        {
            var invoice = BuildInvoiceItem();

            command.CommandText = GetInvoiceInsertSql();

            command.BindParametersFromObject(invoice);

            Assert.Throws<ArgumentNullException>(() =>
            {
                command.RemoveParameter(null);
            });
        }

        #endregion
    }
}
