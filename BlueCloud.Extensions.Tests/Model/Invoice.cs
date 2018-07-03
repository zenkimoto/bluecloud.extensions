using System;
using System.Collections.Generic;
using System.Linq;
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

        public bool ShouldOverridePropertyHydration(string propertyName)
        {
            string[] properties = { "InvoiceDate", "InvoiceId" };
            return properties.Contains(propertyName);
        }

        public object OverridePropertyHydration(string propertyName, object value) {
            switch (propertyName)
            {
                case "InvoiceDate": return ((DateTime)value).ToUniversalTime();
                case "InvoiceId": return 1000 + (long)value;
                default: return null;
            }
        }
    }
}