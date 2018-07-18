using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Data.Common;
using System.Data;
using System.Reflection;
using BlueCloud.Extensions.Collections;
using BlueCloud.Extensions.Assembly;
using System.Diagnostics;

namespace BlueCloud.Extensions.Data
{
    /// <summary>
    /// Extension Methods for System.Data.IDbCommand
    /// </summary>
    public static class IDbCommandExtensions
    {
        /// <summary>
        /// Loads an Embedded Resource into the Command Text
        /// </summary>
        /// <param name="command">IDbCommand</param>
        /// <param name="embeddedResource">Embedded Resource Name</param>
        public static void LoadEmbeddedResource(this IDbCommand command, string embeddedResource)
        {
            command.LoadEmbeddedResource(embeddedResource, System.Reflection.Assembly.GetCallingAssembly());
        }


        /// <summary>
        /// Loads an Embedded Resource into the Command Text
        /// </summary>
        /// <param name="command">IDbCommand</param>
        /// <param name="embeddedResource">Embedded Resource</param>
        /// <param name="assembly">Assembly</param>
        public static void LoadEmbeddedResource(this IDbCommand command, string embeddedResource, System.Reflection.Assembly assembly)
        {
            if (embeddedResource == null)
                throw new ArgumentNullException(nameof(embeddedResource));
            if (assembly == null)
                throw new ArgumentNullException(nameof(assembly));

            string sql = assembly.GetEmbeddedResourceString(embeddedResource);

            command.CommandText = sql;
        }


        /// <summary>
        /// Validates if parameters specified in the SQL Statement matches the parameters added to the Parameter Collection.
        /// This method is not Optimized.  This method is ONLY meant to be executed in DEBUG mode during development.
        /// </summary>
        public static void ValidateParameters(this IDbCommand command)
        {
            if (command.CommandType != CommandType.Text)
            {
                return;
            }

            List<string> sqlStringParameterNames = command.ParameterNamesFromCommandText().Map(x => x.TrimStart(':', '@'));
            List<string> bindParameterNames = command.ParameterNames().Map(x => x.TrimStart(':', '@'));

            sqlStringParameterNames.Sort();
            bindParameterNames.Sort();

            int i = 0;
            int j = 0;

            while (i < sqlStringParameterNames.Count)
            {
                j = 0;
                while (j < bindParameterNames.Count)
                {
                    if (sqlStringParameterNames[i].ToLower() == bindParameterNames[j].ToLower())
                    {
                        sqlStringParameterNames.RemoveAt(i);
                        bindParameterNames.RemoveAt(j);
                        j--;
                        i--;
                        break;
                    }
                    j++;
                }
                i++;
            }

            if (sqlStringParameterNames.Count > 0)
            {
                string sqlParamsMissing = sqlStringParameterNames.Aggregate((x, y) => x + ", " + y);

                var ex = new DataException($"Missing parameters in Command.Parameters array or Model DbField Property: '{sqlParamsMissing}'");
                ex.Data["MissingParams"] = sqlStringParameterNames;

                throw ex;
            }

            if (bindParameterNames.Count > 0)
            {
                string databaseParamsMissing = bindParameterNames.Aggregate((x, y) => x + ", " + y);

                var ex = new DataException($"Parameters missing parameters in SQL String: '{databaseParamsMissing}'");
                ex.Data["MissingParams"] = databaseParamsMissing;

                throw ex;
            }
        }


        /// <summary>
        /// Adds a new database parameter.
        /// </summary>
        /// <param name="command">IDbCommand</param>
        /// <param name="name">Parameter Name</param>
        /// <param name="value">Value</param>
        /// <param name="parameterCallback">Callback to modify additional parameter properties.</param>
        /// <typeparam name="T">Data Type</typeparam>
        public static void AddParameter<T>(this IDbCommand command, string name, T value, Action<IDbDataParameter> parameterCallback = null)
        {
            IDbDataParameter parameter = command.CreateParameter();

            parameter.ParameterName = name ?? throw new ArgumentNullException(nameof(name));
            parameter.Value = value;

            parameterCallback?.Invoke(parameter);

            command.Parameters.Add(parameter);
        }


        /// <summary>
        /// Adds a new output database parameter.
        /// </summary>
        /// <returns>The output parameter.</returns>
        /// <param name="command">IDbCommand</param>
        /// <param name="name">Parameter Name</param>
        /// <param name="type">Database Type</param>
        /// <param name="parameterCallback">Callback to modify additional parameter properties.</param>
        public static void AddOutputParameter(this IDbCommand command, string name, DbType type, Action<IDbDataParameter> parameterCallback = null)
        {
            IDbDataParameter outputParameter = command.CreateParameter();
            outputParameter.Direction = ParameterDirection.Output;
            outputParameter.DbType = type;
            outputParameter.ParameterName = name ?? throw new ArgumentNullException(nameof(name));

            parameterCallback?.Invoke(outputParameter);

            command.Parameters.Add(outputParameter);
        }


        /// <summary>
        /// Creates parameters and binds mapped properties to database parameters.
        /// </summary>
        /// <param name="command">IDbCommand</param>
        /// <param name="model">Model Object to Bind</param>
        /// <typeparam name="T">Data Type</typeparam>
        public static void BindParametersFromObject<T>(this IDbCommand command, T model) where T : class
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            List<string> bindParameters = command.ParameterNamesFromCommandText().Map(x => x.TrimStart(':', '@').ToLower());

            PropertyInfo[] properties = typeof(T).GetProperties();

            foreach (PropertyInfo property in properties)
            {
                var dbField = (DbFieldAttribute)property.GetCustomAttribute(typeof(DbFieldAttribute), true);

                if (dbField == null)
                    continue;

                if (dbField.SqlParameterName == null)
                    continue;

                if (bindParameters.Contains(dbField.SqlParameterName.TrimStart(':', '@').ToLower()) == false)
                    continue;

                var propertyValue = property.GetValue(model);

                // Allow for custom user mapping
                if (model is IDbSerializationOverridable && ((IDbSerializationOverridable)model).ShouldOverridePropertySerialization(property.Name))
                {
                    propertyValue = ((IDbSerializationOverridable)model).OverridePropertySerialization(property.Name, propertyValue);
                }

                Debug.WriteLine($"{property.Name} [{property.PropertyType}] <-> Database Field: {dbField.Field}");

                command.AddParameter(dbField.SqlParameterName, propertyValue);
            }
        }


        /// <summary>
        /// Parameter names in SQL (CommandText)
        /// </summary>
        /// <returns>An enumerable list of parameter names.</returns>
        /// <param name="command">IDbCommand</param>
        public static IEnumerable<string> ParameterNamesFromCommandText(this IDbCommand command)
        {
            var regex = new Regex("[:@][a-zA-Z0-9-_]+");

            MatchCollection matches = regex.Matches(command.CommandText);

            return matches.Map(match => match.Value).AsEnumerable();
        }


        /// <summary>
        /// Parameter names in the Parameters collection.
        /// </summary>
        /// <returns>Enumerable List of Parameter Names</returns>
        /// <param name="command">IDbCommand</param>
        public static IEnumerable<string> ParameterNames(this IDbCommand command)
        {
            var bindParameterNames = new List<string>();

            foreach (DbParameter param in command.Parameters)
            {
                bindParameterNames.Add(param.ParameterName);
            }

            return bindParameterNames.AsEnumerable();
        }


        /// <summary>
        /// Removes a database parameter from the command object.
        /// </summary>
        /// <param name="command">IDbCommand</param>
        /// <param name="parameterName">Parameter name of parameter to remove.</param>
        public static void RemoveParameter(this IDbCommand command, string parameterName)
        {
            if (parameterName == null)
                throw new ArgumentNullException(nameof(parameterName));

            DbParameter paramToDelete = null;

            foreach (DbParameter param in command.Parameters)
            {
                if (param.ParameterName.ToLower() == parameterName.ToLower())
                {
                    paramToDelete = param;
                    break;
                }
            }

            if (paramToDelete != null)
            {
                command.Parameters.Remove(paramToDelete);
            }
        }
    }
}
