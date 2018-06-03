using System;
using System.Collections.Generic;
using System.Reflection;

namespace BlueCloud.Extensions.Data
{
    [Obsolete]
    public static class DbFieldExtensions
    {
        private static readonly Dictionary<Type, IEnumerable<PropertyInfo>> memo = new Dictionary<Type, IEnumerable<PropertyInfo>>();
        private static readonly Dictionary<PropertyInfo, DbFieldAttribute> attribMemo = new Dictionary<PropertyInfo, DbFieldAttribute>();

        [Obsolete]
        public static IEnumerable<PropertyInfo> DbFieldProperties<T>(this T obj)
        {
            Type type = typeof(T);

            if (memo.ContainsKey(type))
            {
                return memo[type];
            }
            
            var dbFieldProperties = new List<PropertyInfo>();

            PropertyInfo[] properties = type.GetProperties();

            foreach (PropertyInfo property in properties)
            {
                if (property.GetDbField() != null)
                {
                    dbFieldProperties.Add(property);
                }
            }

            memo[type] = dbFieldProperties;

            return dbFieldProperties;
        }

        [Obsolete]
        public static DbFieldAttribute GetDbField(this PropertyInfo property)
        {
            if (attribMemo.ContainsKey(property))
            {
                return attribMemo[property];
            }

            DbFieldAttribute attrib = property.GetCustomAttribute<DbFieldAttribute>(true);
            attribMemo[property] = attrib;

            return attrib;
        }
    }
}
