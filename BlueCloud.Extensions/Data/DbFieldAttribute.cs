using System;

namespace BlueCloud.Extensions.Data
{
    /// <summary>
    /// DbField Attribute used to annotate properties to be mapped to database fields.
    /// </summary>
    public class DbFieldAttribute : Attribute
    {
        /// <summary>
        /// Database Field
        /// </summary>
        public string Field { get; private set; }

        /// <summary>
        /// SQL Parameter Name (Optional) in SQL Text
        /// </summary>
        public string SqlParameterName { get; private set; }

        /// <summary>
        /// Database Field to Property Mapping Attribute
        /// 
        /// If fieldName not specified, the database field will be the property name.
        /// </summary>
        /// <param name="fieldName">Database Field</param>
        /// <param name="parameterName">SQL Parameter Name (Optional) in SQL Text</param>
        public DbFieldAttribute(string fieldName, string parameterName = null)
        {
            Field = fieldName;
            SqlParameterName = parameterName ?? fieldName;
        }

        /// <summary>
        /// Database Field to Property Mapping Attribute.
        /// 
        /// If fieldName not specified, the database field will be the property name.
        /// </summary>
        public DbFieldAttribute()
        {
            Field = null;
            SqlParameterName = null;
        }
    }
}
