﻿using System;
using System.IO;
using System.Text.RegularExpressions;

namespace BlueCloud.Extensions.Assembly
{
    /// <summary>
    /// Extension Methods for System.Reflection.Assembly
    /// </summary>
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
                throw new ArgumentNullException(nameof(name));

            string pattern = $"^([A-Za-z-_0-9]+\\.)*{name}$";

            var regex = new Regex(pattern);

            foreach (string resourceName in assembly.GetManifestResourceNames())
            {
                if (regex.Match(resourceName).Success)
                {
                    return new StreamReader(assembly.GetManifestResourceStream(resourceName));
                }
            }

            throw new FileNotFoundException($"Embedded Resource: '{name}' not found.  If the file exists in the project, check if the file is marked as an Embedded Resource.");
        }


        /// <summary>
        /// Gets an embedded resource string of text file embedded into Assembly.
        /// </summary>
        /// <param name="name">Name of embedded resource</param>
        /// <returns>string</returns>
        public static string GetEmbeddedResourceString(this System.Reflection.Assembly assembly, string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            using (StreamReader reader = assembly.GetEmbeddedResourceStream(name))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
