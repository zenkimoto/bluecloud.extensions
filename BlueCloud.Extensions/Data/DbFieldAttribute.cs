using System;

namespace BlueCloud.Extensions.Data
{
    public class DbFieldAttribute: Attribute
    {
        public string Field { get; private set; }
        public string SqlParameterName { get; private set; }

        /// <summary>
        /// Database Field to Property Mapping Attribute
        /// </summary>
        /// <param name="fieldName">Database Field</param>
        /// <param name="parameterName">SQL Parameter Name (Optional)</param>
        public DbFieldAttribute(string fieldName, string parameterName = null)
        {
            Field = fieldName;
            SqlParameterName = parameterName ?? fieldName;
        }
    }
}
