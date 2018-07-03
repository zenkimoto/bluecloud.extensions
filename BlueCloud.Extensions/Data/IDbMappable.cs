using System;
namespace BlueCloud.Extensions.Data
{
    public interface IDbHydrationOverridable
    {
        bool ShouldOverridePropertyHydration(string propertyName);
        object OverridePropertyHydration(string propertyName, object value);
    }

    public interface IDbSerializationOverridable
    {
        bool ShouldOverridePropertySerialization(string propertyName);
        object OverridePropertySerialization(string propertyName, object value);
    }
}
