﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Data.Common;
using System.Data;
using System.Reflection;
using System.Text;
using BlueCloud.Extensions.Collections;
using BlueCloud.Extensions.Assembly;

namespace BlueCloud.Extensions.Data
{
    public static class IDbCommandExtensions
    {
        /// <summary>
        /// Loads an Embedded Resource into the Command Text
        /// </summary>
        /// <param name="command">IDbCommand</param>
        /// <param name="embeddedResource">Embedded Resource</param>
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
            string sql = assembly.GetEmbeddedResourceString(embeddedResource);

            command.CommandText = sql;
        }


        /// <summary>
        /// Validates if parameters specified in the SQL Statement matches the parameters added to the Parameter Collection.
        /// Not Optimized.  This method is meant to be executed in DEBUG mode.
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
        /// Adds the parameter.
        /// </summary>
        /// <param name="command">Command.</param>
        /// <param name="name">Name.</param>
        /// <param name="value">Value.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static void AddParameter<T>(this IDbCommand command, string name, T value)
        {
            IDbDataParameter parameter = command.CreateParameter();

            parameter.ParameterName = name;

            Type underlyingType = Nullable.GetUnderlyingType(typeof(T));
            bool isNullableType = underlyingType != null;

            // TODO: Use Custom Mapping Instead

            if (value is bool)
            {                
                parameter.Value = (bool)Convert.ChangeType(value, typeof(bool)) ? 1 : 0;
            } 
            else
            {
                parameter.Value = value;
            }

            command.Parameters.Add(parameter);
        }


        /// <summary>
        /// Adds the output parameter.
        /// </summary>
        /// <returns>The output parameter.</returns>
        /// <param name="command">Command.</param>
        /// <param name="name">Name.</param>
        /// <param name="type">Type.</param>
        public static IDbDataParameter AddOutputParameter(this IDbCommand command, string name, DbType type)
        {
            IDbDataParameter outputParameter = command.CreateParameter();
            outputParameter.Direction = ParameterDirection.Output;
            outputParameter.DbType = type;
            outputParameter.ParameterName = name;

            command.Parameters.Add(outputParameter);

            return outputParameter;
        }


        /// <summary>
        /// Adds the parameter collection.
        /// </summary>
        /// <param name="command">Command.</param>
        /// <param name="name">Name.</param>
        /// <param name="type">Type.</param>
        /// <param name="collection">Collection.</param>
        /// <typeparam name="TValue">The 1st type parameter.</typeparam>
        public static void AddParameterCollection<TValue>(this IDbCommand command, string name, DbType type, IEnumerable<TValue> collection)
        {
            if (name == null || collection == null) return;

            var parameters = new List<IDbDataParameter>();
            var counter = 0;
            var parameterBuilder = new StringBuilder(":");

            foreach (var obj in collection)
            {
                var parameterName = name + counter;
                parameterBuilder.Append(parameterName);
                parameterBuilder.Append(", :");

                IDbDataParameter parameter = command.CreateParameter();
                parameter.DbType = type;
                parameter.ParameterName = parameterName;
                parameter.Value = obj;
                parameters.Add(parameter);
                
                counter++;
            }

            parameterBuilder.Remove(parameterBuilder.Length - 3, 3);
            command.CommandText = command.CommandText.ToUpper().Replace(":" + name.ToUpper(), parameterBuilder.ToString());

            foreach (IDbDataParameter parameter in parameters)
            {
                command.Parameters.Add(parameter);
            }
        }


        /// <summary>
        /// Binds the parameters from object.
        /// </summary>
        /// <param name="command">Command.</param>
        /// <param name="model">Model.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static void BindParametersFromObject<T>(this IDbCommand command, T model)
        {
            List<string> parameters = command.ParameterNamesFromCommandText();
            Dictionary<string, PropertyInfo> propertyDictionary = model.DbFieldProperties().ToDictionary(x => x.GetDbField().SqlParameterName.ToUpper());

            foreach (string paramName in parameters)
            {
                string paramDictKey = paramName.Trim('@', ':').ToUpper();

                if (propertyDictionary.ContainsKey(paramDictKey))
                {
                    PropertyInfo property = propertyDictionary[paramDictKey];

                    var dbField = (DbFieldAttribute)property.GetCustomAttribute(typeof(DbFieldAttribute), true);

                    command.AddParameter(paramName, property.GetValue(model));
                }
            }
        }


        /// <summary>
        /// Parameters the names from command text.
        /// </summary>
        /// <returns>The names from command text.</returns>
        /// <param name="command">Command.</param>
        public static List<string> ParameterNamesFromCommandText(this IDbCommand command)
        {
            var regex = new Regex("[:@][a-zA-Z0-9-_]+");

            MatchCollection matches = regex.Matches(command.CommandText);

            return matches.Map(match => match.Value);
        }


        /// <summary>
        /// Parameters the names.
        /// </summary>
        /// <returns>The names.</returns>
        /// <param name="command">Command.</param>
        public static List<string> ParameterNames(this IDbCommand command)
        {
            var bindParameterNames = new List<string>();

            foreach (DbParameter param in command.Parameters)
            {
                bindParameterNames.Add(param.ParameterName);
            }

            return bindParameterNames;
        }


        /// <summary>
        /// Removes the parameter.
        /// </summary>
        /// <param name="command">Command.</param>
        /// <param name="parameterName">Parameter name.</param>
        public static void RemoveParameter(this IDbCommand command, string parameterName)
        {
            // TODO: Improve
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
