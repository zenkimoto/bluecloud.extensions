using BlueCloud.Extensions.Data;
using Sample2.Models;
using System;
using System.Collections.Generic;

namespace Sample2.DataAccess.InvoiceItems
{
    public class InvoiceItemDao : BaseDao
    {
        public InvoiceItemDao(string connectionString) : base(connectionString)
        {
        }

        public IEnumerable<InvoiceItem> GetInvoiceItemsByInvoiceId(long invoiceId)
        {
            return OpenDatabaseConnection(connection =>
            {
                // Example: Using an embedded resource file and passing in a parameter
                return connection.GetObjectsFromEmbeddedResource<InvoiceItem>("InvoiceItemDao_GetInvoiceItemsByInvoiceId.sql", command => 
                {
                    command.AddParameter("InvoiceId", invoiceId);
                });
            });
        }
    }
}
