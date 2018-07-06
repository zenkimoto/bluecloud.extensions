using System;
using System.Text.RegularExpressions;

namespace BlueCloud.Extensions.String
{
    public static class StringExtensions
    {
        /// <summary>
        /// Checks if string matches regular expression.  Returns true if a match is found.
        /// </summary>
        /// <param name="str">String to Check</param>
        /// <param name="pattern">Regular Expression Pattern</param>
        /// <returns></returns>
        public static bool MatchesRegularExpression(this string str, string pattern)
        {
            if (pattern == null)
                throw new ArgumentNullException(nameof(pattern));

            var regex = new Regex(pattern);

            return regex.Match(str).Success;
        }
    }
}
