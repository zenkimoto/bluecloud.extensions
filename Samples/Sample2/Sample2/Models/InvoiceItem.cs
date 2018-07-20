using System.ComponentModel.DataAnnotations;
using BlueCloud.Extensions.Data;

namespace Sample2.Models
{
    public class InvoiceItem
    {
        // In this model, the property names match the database fields.
        // In this situation, you can omit the Field parameter.

        [DbField]
        public long InvoiceLineId { get; set; }

        [DbField]
        public long InvoiceId { get; set; }

        [DbField]
        public long TrackId { get; set; }

        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:c}")]
        [DbField]
        public double UnitPrice { get; set; }

        [DbField]
        public long Quantity { get; set; }

        [DbField]
        public string TrackName { get; set; }

        [DbField]
        public string AlbumTitle { get; set; }

        [DbField]
        public string ArtistName { get; set; }

        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:c}")]
        public double LineItemTotal
        {
            get
            {
                return UnitPrice * Quantity;
            }
        }
    }
}
