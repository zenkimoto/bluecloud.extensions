using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace BlueCloud.Extensions.Collections
{
    /// <summary>
    /// Extension Methods for System.Text.RegularExpressions.MatchCollection
    /// </summary>
    public static class MatchCollectionExtensions
    {
        /// <summary>
        /// Map a Regular Expression MatchCollection into a List<typeparamref name="T"/>
        /// </summary>
        /// <returns>Mapped List</returns>
        /// <param name="collection">Match Collection</param>
        /// <param name="func">Function to map values</param>
        /// <typeparam name="T">Type</typeparam>
        public static List<T> Map<T>(this MatchCollection collection, Func<Match, T> func)
        {
            var result = new List<T>();

            foreach (Match match in collection)
            {
                result.Add(func(match));
            }

            return result;
        }
    }

    /// <summary>
    /// Extension Methods for the IEnumerable Interface.
    /// </summary>
    public static class IEnumerableExtensions
    {
        /// <summary>
        /// Map an IEnumerable into a List<typeparamref name="T"/>
        /// </summary>
        /// <returns>Mapped List</returns>
        /// <param name="collection">IEnumerable</param>
        /// <param name="func">Function to map values</param>
        /// <typeparam name="T">Original Type</typeparam>
        /// <typeparam name="U">Mapped Type</typeparam>
        public static List<U> Map<T, U>(this IEnumerable<T> collection, Func<T, U> func)
        {
            var result = new List<U>();

            foreach (T obj in collection)
            {
                result.Add(func(obj));
            }

            return result;
        }

        /// <summary>
        /// Loops through each element in an IEnumerable and performs an action.
        /// </summary>
        /// <param name="enumeration">IEnumerable</param>
        /// <param name="action">Action to execute against each element</param>
        /// <typeparam name="T">Type</typeparam>
        public static void ForEach<T>(this IEnumerable<T> enumeration, Action<T> action)
        {
            foreach (T item in enumeration)
            {
                action(item);
            }
        }
    }
}
