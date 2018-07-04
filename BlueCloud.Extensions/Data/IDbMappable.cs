using System;
namespace BlueCloud.Extensions.Data
{
    public interface IDbHydrationOverridable
    {
        /// <summary>
        /// Shoulds the override property hydration.
        /// </summary>
        /// <returns><c>true</c>, if override property hydration was shoulded, <c>false</c> otherwise.</returns>
        /// <param name="propertyName">Property name.</param>
        bool ShouldOverridePropertyHydration(string propertyName);

        /// <summary>
        /// Overrides the property hydration.
        /// </summary>
        /// <returns>The property hydration.</returns>
        /// <param name="propertyName">Property name.</param>
        /// <param name="value">Value.</param>
        object OverridePropertyHydration(string propertyName, object value);
    }

    public interface IDbSerializationOverridable
    {
        /// <summary>
        /// Shoulds the override property serialization.
        /// </summary>
        /// <returns><c>true</c>, if override property serialization was shoulded, <c>false</c> otherwise.</returns>
        /// <param name="propertyName">Property name.</param>
        bool ShouldOverridePropertySerialization(string propertyName);

        /// <summary>
        /// Overrides the property serialization.
        /// </summary>
        /// <returns>The property serialization.</returns>
        /// <param name="propertyName">Property name.</param>
        /// <param name="value">Value.</param>
        object OverridePropertySerialization(string propertyName, object value);
    }
}
