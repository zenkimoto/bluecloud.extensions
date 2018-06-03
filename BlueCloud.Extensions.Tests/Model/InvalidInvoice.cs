using System;
using BlueCloud.Extensions.Data;

namespace BlueCloud.Extensions.Tests.Model
{
    public class InvalidInvoice
    {
        [DbField("EmployeeId")]
        public long InvoiceId { get; set; }

        [DbField("CustomerId")]
        public long CustomerId { get; set; }

        [DbField("InvoiceDate")]
        public DateTime InvoiceDate { get; set; }
    }
}
