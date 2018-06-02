using System;
using System.IO;
using BlueCloud.Extensions.String;

namespace BlueCloud.Extensions.Assembly
{
    public static class AssemblyExtensions
    {
        /// <summary>
        /// Gets an embedded resource stream of file embedded into Assembly.
        /// </summary>
        /// <param name="name">Name of embedded resource</param>
        /// <returns>string</returns>
        public static StreamReader GetEmbeddedResourceStream(this System.Reflection.Assembly assembly, string name)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            string pattern = $"^([A-Za-z-_0-9]+\\.)*{name}$";

            foreach (string resourceName in assembly.GetManifestResourceNames())
            {
                if (resourceName.MatchesRegularExpression(pattern))
                {
                    return new StreamReader(assembly.GetManifestResourceStream(resourceName));
                }
            }

            throw new Exception($"Embedded Resource: '{name}' not found.");
        }


        /// <summary>
        /// Gets an embedded resource string of text file embedded into Assembly.
        /// </summary>
        /// <param name="name">Name of embedded resource</param>
        /// <returns>string</returns>
        public static string GetEmbeddedResourceString(this System.Reflection.Assembly assembly, string name)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            using (StreamReader reader = assembly.GetEmbeddedResourceStream(name))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
