using System;
using System.Collections.Generic;
using System.Linq;
using BlueCloud.Extensions.Data;

namespace BlueCloud.Extensions.Tests.Model
{
    public class Invoice : IDbHydrationOverridable, IDbSerializationOverridable
    {
        [DbField("InvoiceId")]
        public long InvoiceId { get; set; }

        [DbField("CustomerId")]
        public long CustomerId { get; set; }

        [DbField("InvoiceDate")]
        public DateTime InvoiceDate { get; set; }

        [DbField("BillingState")]
        public string BillingState { get; set; }

        public bool ShouldOverridePropertyHydration(string propertyName)
        {
            string[] properties = { "InvoiceDate", "InvoiceId" };
            return properties.Contains(propertyName);
        }

        public object OverridePropertyHydration(string propertyName, object value)
        {
            switch (propertyName)
            {
                case "InvoiceDate": return ((DateTime)value).ToUniversalTime();
                case "InvoiceId": return 1000 + (long)value;
                default: return null;
            }
        }

        public bool ShouldOverridePropertySerialization(string propertyName)
        {
            // Override all values...
            return true;
        }

        public object OverridePropertySerialization(string propertyName, object value)
        {
            switch (propertyName)
            {
                case "InvoiceId": return 1000 + (long)value;
                case "CustomerId": return 1000 + (long)value;
                case "InvoiceDate": return ((DateTime)value).AddDays(31);
                default: return null;
            }
        }
    }
}