﻿using System;
using Xunit;
using BlueCloud.Extensions.Data;
using Microsoft.Data.Sqlite;
using BlueCloud.Extensions.Tests.Model;
using System.Linq;
using System.Data;

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
        public void PopulateProperties_ShouldPopulateAttributedProperties()
        {
            QueryAlbums();
            
            var album = new Album();

            reader.Read();

            reader.PopulateProperties<Album>(album);

            Assert.Equal(1, album.AlbumId);
            Assert.Equal("For Those About To Rock We Salute You", album.Title);
            Assert.Equal(1, album.ArtistId);
        }

        [Fact]
        public void MapToObjects_ShouldSetNullableValuesToNull()
        {
            QueryEmployees();

            var employees = reader.MapToObjects<Employee>();

            Assert.Equal(null, employees.First()?.ReportsTo);
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

            Assert.Equal(1, employees.Count());
        }

        [Fact]
        public void MapToObjects_WhenTakingMoreThanNumberOfRows_ShouldReturnWhatsInDatabase()
        {
            QueryEmployees();

            var employees = reader.MapToObjects<Employee>(100);

            Assert.Equal(8, employees.Count());
        }

        [Fact]
        public void MapToObjects_WhenAttemptingToMapToInvalidMapping_ShouldThrowInvalidCastException()
        {
            QueryEmployees();

            Assert.Throws(typeof(InvalidCastException), () =>
            {
                reader.MapToObjects<InvalidEmployee>(1);
            });
        }

        [Fact]
        public void MapToObjects_CustomOverride()
        {
            QueryInvoices();

            var invoices = reader.MapToObjects<Invoice>(1);

            Assert.Equal(new DateTime(2009, 1, 1, 8, 0, 0, DateTimeKind.Utc), invoices.First().InvoiceDate);
            Assert.Equal(1001, invoices.First().InvoiceId);
        }
    }
}
