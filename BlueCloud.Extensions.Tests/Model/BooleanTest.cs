using System;
using BlueCloud.Extensions.Data;

namespace BlueCloud.Extensions.Tests.Model
{
    public class BooleanTest : IDbHydrationOverridable, IDbSerializationOverridable
    {
        [DbField("boolean_value")]
        public bool BooleanValue { get; set; }

        [DbField("long_value")]
        public long LongValue { get; set; }

        public BooleanTest() 
        {
            BooleanValue = true;
            LongValue = 123;
        }

        #region IDbHydrationOverridable

        public bool ShouldOverridePropertyHydration(string propertyName)
        {
            return propertyName == "BooleanValue" ? true : false;
        }

        public object OverridePropertyHydration(string propertyName, object value)
        {
            // This method only gets called when the propertyName is BooleanValue.

            return (long)value == 1 ? true : false;  // Hydrate property value to C# Boolean Type
        }

        #endregion

        #region IDbSerializationOverridable

        public bool ShouldOverridePropertySerialization(string propertyName)
        {
            return propertyName == "BooleanValue" ? true : false;
        }

        public object OverridePropertySerialization(string propertyName, object value)
        {
            // This method only gets called when the propertyName is BooleanValue.

            return (bool)value == true ? 1 : 0; // Persist to the database as a database NUMBER type
        }

        #endregion
    }
}
