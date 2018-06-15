using System;
using BlueCloud.Extensions.Data;

namespace BlueCloud.Extensions.Tests.Model
{
    public class Invoice : IDbHydrationOverridable
    {
        [DbField("InvoiceId")]
        public long InvoiceId { get; set; }

        [DbField("CustomerId")]
        public long CustomerId { get; set; }

        [DbField("InvoiceDate")]
        public DateTime InvoiceDate { get; set; }

        public bool ShouldOverridePropertyHydration(string propertyName, object value)
        {
            switch (propertyName)
            {
                case "InvoiceDate":
                    InvoiceDate = ((DateTime)value).ToUniversalTime();
                    return true;
                case "InvoiceId":
                    InvoiceId = 1000 + (long)value;
                    return true;
            }

            return false;
        }
    }
}