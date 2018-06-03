using System;
namespace BlueCloud.Extensions.Data
{
    public interface IDbMappable {
        bool ShouldOverrideDatabaseMapping(string propertyName, object value);
    }
}
