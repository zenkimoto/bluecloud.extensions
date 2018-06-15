using System;
namespace BlueCloud.Extensions.Data
{
    //https://www.bricelam.net/2015/12/10/shareable-in-memory-databases.html
    //https://www.connectionstrings.com/sqlite/

    [Obsolete]
    public interface IDbMappable {
        [Obsolete]
        bool ShouldOverrideDatabaseMapping(string propertyName, object value);
    }

    public interface IDbHydrationOverridable
    {
        bool ShouldOverridePropertyHydration(string propertyName, object value);
    }

    public interface IDbSerializationOverridable
    {
        bool ShouldOverridePropertySerialization(string propertyName, object value);
    }
}
