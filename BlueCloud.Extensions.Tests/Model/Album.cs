using System;
using BlueCloud.Extensions.Data;

namespace BlueCloud.Extensions.Tests.Model
{
    public class Album
    {
        [DbField("AlbumId")]
        public int AlbumId { get; set; }

        [DbField("Title")]
        public string Title { get; set; }

        [DbField("ArtistId")]
        public int ArtistId { get; set; }
    }
}
