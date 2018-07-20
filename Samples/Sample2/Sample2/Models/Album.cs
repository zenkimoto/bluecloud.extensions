using BlueCloud.Extensions.Data;

namespace Sample2.Models
{
    public class Album
    {
        // Simple example of field mapping

        [DbField("AlbumId")]
        public long AlbumId { get; set; }

        [DbField("Title")]
        public string Title { get; set; }

        [DbField("ArtistId")]
        public long ArtistId { get; set; }
    }
}
