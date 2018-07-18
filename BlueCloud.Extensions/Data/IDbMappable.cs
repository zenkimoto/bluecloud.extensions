namespace BlueCloud.Extensions.Data
{
    /// <summary>
    /// IDbHydrationOverridable used to customize property hydration.
    /// </summary>
    public interface IDbHydrationOverridable
    {
        /// <summary>
        /// Should the model override property hydration.
        /// </summary>
        /// <returns><c>true</c>, if override property hydration, <c>false</c> otherwise.</returns>
        /// <param name="propertyName">Property Name</param>
        bool ShouldOverridePropertyHydration(string propertyName);

        /// <summary>
        /// Overrides the property hydration.
        /// </summary>
        /// <returns>New Value</returns>
        /// <param name="propertyName">Property Name</param>
        /// <param name="value">Original Value</param>
        object OverridePropertyHydration(string propertyName, object value);
    }

    /// <summary>
    /// IDbSerializationOverridable used to customize property database serialization.
    /// </summary>
    public interface IDbSerializationOverridable
    {
        /// <summary>
        /// Should the model override property serialization.
        /// </summary>
        /// <returns><c>true</c>, if override property serialization, <c>false</c> otherwise.</returns>
        /// <param name="propertyName">Property Name</param>
        bool ShouldOverridePropertySerialization(string propertyName);

        /// <summary>
        /// Overrides the property serialization.
        /// </summary>
        /// <returns>New Value</returns>
        /// <param name="propertyName">Property Name</param>
        /// <param name="value">Original Value</param>
        object OverridePropertySerialization(string propertyName, object value);
    }
}
