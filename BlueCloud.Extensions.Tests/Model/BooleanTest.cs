using System;
using BlueCloud.Extensions.Data;

namespace BlueCloud.Extensions.Tests.Model
{
    public class BooleanTest : IDbHydrationOverridable, IDbSerializationOverridable
    {
        [DbField("boolean_value")]
        public bool BooleanValue { get; set; }

        #region IDbHydrationOverridable

        public bool ShouldOverridePropertyHydration(string propertyName)
        {
            return true;
        }

        public object OverridePropertyHydration(string propertyName, object value)
        {
            return (long)value == 1 ? true : false;  // Hydrate property value to C# Boolean Type
        }

        #endregion

        #region IDbSerializationOverridable

        public bool ShouldOverridePropertySerialization(string propertyName)
        {
            return true;
        }

        public object OverridePropertySerialization(string propertyName, object value)
        {
            return (bool)value == true ? 1 : 0; // Persist to the database as a database NUMBER type
        }

        #endregion
    }
}
