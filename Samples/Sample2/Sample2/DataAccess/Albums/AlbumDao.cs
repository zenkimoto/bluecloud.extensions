using System.Collections.Generic;
using BlueCloud.Extensions.Data;
using Sample2.Models;

namespace Sample2.DataAccess.Albums
{
    public class AlbumDao: BaseDao
    {
        public AlbumDao(string connectionString) : base(connectionString)
        {
        }

        public IEnumerable<Album> GetAlbums()
        {
            return OpenDatabaseConnection(connection =>
            {
                // GetObjectsFromQueryString example for simple queries
                return connection.GetObjectsFromQueryString<Album>("SELECT * FROM Albums");
            });
        }
    }
}
