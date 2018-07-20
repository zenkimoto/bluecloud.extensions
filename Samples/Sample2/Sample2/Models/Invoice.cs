using BlueCloud.Extensions.Data;
using System;
using System.ComponentModel.DataAnnotations;

namespace Sample2.Models
{
    public class Invoice
    {
        [DbField("InvoiceId")]
        public long InvoiceId { get; set; }

        [DbField("CustomerId")]
        public long CustomerId { get; set; }

        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        [DbField("InvoiceDate")]
        public DateTime InvoiceDate { get; set; }

        [DbField("BillingAddress")]
        public string BillingAddress { get; set; }

        [DbField("BillingCity")]
        public string BillingCity { get; set; }

        [DbField("BillingState")]
        public string BillingState { get; set; }

        [DbField("BillingCountry")]
        public string BillingCountry { get; set; }

        [DbField("BillingPostalCode")]
        public string BillingPostalCode { get; set; }

        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:c}")]
        [DbField("Total")]
        public double Total { get; set; }
    }
}
