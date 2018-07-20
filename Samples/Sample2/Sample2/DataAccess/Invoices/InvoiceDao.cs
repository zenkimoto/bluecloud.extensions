using BlueCloud.Extensions.Data;
using Sample2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sample2.DataAccess.Invoices
{
    public class InvoiceDao : BaseDao
    {
        public InvoiceDao(string connectionString) : base(connectionString)
        {
        }

        public IEnumerable<Invoice> GetInvoices()
        {
            return OpenDatabaseConnection(connection =>
            {
                // Example: Using an embedded resource file to store SQL
                return connection.GetObjectsFromEmbeddedResource<Invoice>("InvoiceDao_GetInvoices.sql");
            });
        }
    }
}
