using System;

namespace BlueCloud.Extensions.Data
{
    public class DbFieldAttribute: Attribute
    {
        public string Field { get; private set; }
        public string SqlParameterName { get; private set; }

        public DbFieldAttribute(string fieldName, string parameterName = null)
        {
            this.Field = fieldName;
            this.SqlParameterName = parameterName ?? fieldName;
        }
    }
}
