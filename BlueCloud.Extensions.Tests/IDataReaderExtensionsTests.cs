using System;
using Xunit;
using BlueCloud.Extensions.Data;
using Microsoft.Data.Sqlite;
using BlueCloud.Extensions.Tests.Model;
using System.Linq;
using System.Data;
using BlueCloud.Extensions.Tests.Database;

namespace BlueCloud.Extensions.Tests
{
    public class IDataReaderExtensionsTests : IDisposable
    {
        SqliteConnection connection;
        SqliteCommand command;
        IDataReader reader;

        public IDataReaderExtensionsTests()
        {
            connection = new SqliteConnection("Data Source=:memory:");
            connection.Open();

            InMemoryDatabase.Setup(connection);
        }

        public void Dispose()
        {
            reader?.Close();
            reader?.Dispose();

            command?.Dispose();

            connection.Close();
            connection.Dispose();
            connection = null;
        }

        #region Helper Functions

        private void QueryEmployees()
        {
            command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM employees";

            reader = command.ExecuteReader();
        }

        private void QueryAlbums()
        {
            command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM albums";

            reader = command.ExecuteReader();
        }

        private void QueryInvoices()
        {
            command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM invoices";

            reader = command.ExecuteReader();
        }

        private void QueryJoin()
        {
            command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM albums, artists WHERE albums.ArtistId = artists.ArtistId";

            reader = command.ExecuteReader();
        }

        private void SetupInvalidNumberTest()
        {
            command = connection.CreateCommand();
            command.CommandText = "CREATE TABLE NUMBER_TEST (ID NUMBER NOT NULL, VALUE NUMBER)";
            command.ExecuteNonQuery();
            command.Dispose();

            command = connection.CreateCommand();
            command.CommandText = "INSERT INTO NUMBER_TEST (ID, VALUE) VALUES (1, NULL)";
            command.ExecuteNonQuery();

            command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM NUMBER_TEST";
            reader = command.ExecuteReader();
        }

        #endregion


        #region GetValue Tests

        [Fact]
        public void GetValue_ShouldReturnCorrectValue()
        {
            QueryAlbums();

            reader.Read();

            var albumId = reader.GetValue<int>("AlbumId");
            var title = reader.GetValue<string>("Title");
            var artistId = reader.GetValue<int>("ArtistId");

            Assert.Equal(1, albumId);
            Assert.Equal("For Those About To Rock We Salute You", title);
            Assert.Equal(1, artistId);
        }

        [Fact]
        public void GetValue_WhenAssigningToNullable_ShouldReturnCorrectValue()
        {
            QueryAlbums();

            reader.Read();

            var albumId = reader.GetValue<int?>("AlbumId");

            Assert.Equal(1, albumId.Value);
        }

        [Fact]
        public void GetValue_WhenNullParameterName_ShouldThrowArgumentNullException()
        {
            QueryAlbums();

            reader.Read();

            Assert.Throws<ArgumentNullException>(() =>
            {
                reader.GetValue<int>(null);
            });
        }

        [Fact]
        public void GetValue_WhenAttemptingToAssignNullToAValueType_ShouldThrowException()
        {
            QueryInvoices();

            reader.Read();

            Assert.Throws<InvalidOperationException>(() =>
            {
                reader.GetValue<int>("BillingState");
            });
        }

        [Fact]
        public void GetValue_WhenAttemptingToAssignNullToAReferenceType_ShouldAssignSuccessfully()
        {
            QueryInvoices();

            reader.Read();

            string billingState = reader.GetValue<string>("BillingState");

            Assert.Null(billingState);
        }

        #endregion


        #region MapToObjects Tests

        [Fact]
        public void MapToObjects_ShouldSetNullableValuesToNull()
        {
            QueryEmployees();

            var employees = reader.MapToObjects<Employee>();

            Assert.Null(employees.First()?.ReportsTo);
        }

        [Fact]
        public void MapToObjects_PopulateObjectsCorrectly()
        {
            QueryEmployees();

            var employees = reader.MapToObjects<Employee>();

            Assert.Equal(1, employees.First().EmployeeId);
            Assert.Equal(8, employees.Last().EmployeeId);
            Assert.Equal(8, employees.Count());
        }

        [Fact]
        public void MapToObjects_WhenTakingOne_ShouldReturnOneObject()
        {
            QueryEmployees();

            var employees = reader.MapToObjects<Employee>(1);

            Assert.Single(employees);
        }

        [Fact]
        public void MapToObjects_WhenTakingMoreThanNumberOfRows_ShouldReturnWhatsInDatabase()
        {
            QueryEmployees();

            var employees = reader.MapToObjects<Employee>(100);

            Assert.Equal(8, employees.Count());
        }

        [Fact]
        public void MapToObjects_WhenMappingToOneRowWithTwoDifferentObjectsWithAOneToOneJoin_ShouldReturnATuple()
        {
            QueryJoin();

            var tuple = reader.MapToObjects<Album, Artist>(1).First();

            Assert.Equal(1, tuple.Item1.AlbumId);
            Assert.Equal("For Those About To Rock We Salute You", tuple.Item1.Title);
            Assert.Equal(1, tuple.Item1.ArtistId);

            Assert.Equal(1, tuple.Item2.ArtistId);
            Assert.Equal("AC/DC", tuple.Item2.Name);
        }

        [Fact]
        public void MapToObjects_WhenMappingToTwoDifferentObjectsWithAOneToOneJoin_ShouldReturnTuples()
        {
            QueryJoin();

            var tuple = reader.MapToObjects<Album, Artist>().Last();

            Assert.Equal(347, tuple.Item1.AlbumId);
            Assert.Equal("Koyaanisqatsi (Soundtrack from the Motion Picture)", tuple.Item1.Title);
            Assert.Equal(275, tuple.Item1.ArtistId);

            Assert.Equal(275, tuple.Item2.ArtistId);
            Assert.Equal("Philip Glass Ensemble", tuple.Item2.Name);
        }

        [Fact]
        public void MapToObjects_WhenAttemptingToAssignNullToAReferenceField_ShouldAssignNull()
        {
            QueryInvoices();

            var invoice = reader.MapToObjects<Invoice>(1).First();

            Assert.Null(invoice.BillingState);
        }

        [Fact]
        public void MapToObjects_WhenAttemptingToMapToInvalidMapping_ShouldThrowInvalidCastException()
        {
            QueryEmployees();

            Assert.Throws<InvalidCastException>(() =>
            {
                reader.MapToObjects<InvalidEmployee>(1);
            });
        }

        [Fact]
        public void MapToObjects_WhenCustomOverridingMapping_ShouldMapCorrectly()
        {
            QueryInvoices();

            var invoices = reader.MapToObjects<Invoice>(1);

            Assert.Equal(new DateTime(2009, 1, 1, 8, 0, 0, DateTimeKind.Utc), invoices.First().InvoiceDate);
            Assert.Equal(1001, invoices.First().InvoiceId);
            Assert.Equal(2, invoices.First().CustomerId);
        }

        [Fact]
        public void MapToObjects_WhenAttemptingToMapToAnInvalidDatabaseField_ShouldThrowInvalidOperationException()
        {
            QueryInvoices();

            Assert.Throws<InvalidOperationException>(() =>
            {
                reader.MapToObjects<InvalidInvoice>(1);
            });
        }

        [Fact]
        public void MapToObjects_WhenAttemptingToMapNullableToNonNullableField_ShouldThrowInvalidOperationException()
        {
            SetupInvalidNumberTest();

            Assert.Throws<InvalidCastException>(() =>
            {
                reader.MapToObjects<NumberTest>();
            });
        }

        #endregion


        #region GetColumnOrdinals Tests

        [Fact]
        public void GetColumnOrdinals_ShouldReturnColumnOrdinalDictionary()
        {
            QueryAlbums();

            reader.Read();

            var columnOrdinals = reader.GetColumnOrdinals();

            Assert.Equal(0, columnOrdinals["albumid"]);
            Assert.Equal(1, columnOrdinals["title"]);
            Assert.Equal(2, columnOrdinals["artistid"]);
            Assert.Equal(3, columnOrdinals.Count);
        }

        #endregion
    }
}
