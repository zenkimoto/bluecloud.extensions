using BlueCloud.Extensions.Data;

namespace BlueCloud.Extensions.Tests.Model
{
    public class Artist
    {
        [DbField("ArtistId")]
        public long ArtistId { get; set; }

        [DbField("Name")]
        public string Name { get; set; }
    }
}
