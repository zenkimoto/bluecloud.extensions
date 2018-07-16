using System.Reflection;

namespace BlueCloud.Extensions.Data
{
    public class DbMapping
    {
        public string DatabaseField { get; set; }
        public PropertyInfo ObjectProperty { get; set; }
        public bool IsNullable { get; set; }
    }
}
