using System.Reflection;

namespace BlueCloud.Extensions.Data
{
    /// <summary>
    /// Db mapping.
    /// </summary>
    public class DbMapping
    {
        /// <summary>
        /// Mapped Database Field
        /// </summary>
        /// <value>The database field.</value>
        public string DatabaseField { get; set; }

        /// <summary>
        /// Reflected Object Property
        /// </summary>
        /// <value>PropertyInfo of a property</value>
        public PropertyInfo ObjectProperty { get; set; }

        /// <summary>
        /// Whether the Reflected Object Property is a Nullable type.
        /// </summary>
        /// <value>Whether or not the data type is nullable.</value>
        public bool IsNullable { get; set; }
    }
}
