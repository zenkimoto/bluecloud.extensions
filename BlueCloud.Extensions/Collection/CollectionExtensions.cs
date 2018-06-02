using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace BlueCloud.Extensions.Collections
{
    public static class MatchCollectionExtensions
    {
        /// <summary>
        /// Map the specified collection and func.
        /// </summary>
        /// <returns>The map.</returns>
        /// <param name="collection">Collection.</param>
        /// <param name="func">Func.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
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

    public static class IEnumerableExtensions
    {
        /// <summary>
        /// Map the specified collection and func.
        /// </summary>
        /// <returns>The map.</returns>
        /// <param name="collection">Collection.</param>
        /// <param name="func">Func.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        /// <typeparam name="U">The 2nd type parameter.</typeparam>
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
        /// Tos the dictionary.
        /// </summary>
        /// <returns>The dictionary.</returns>
        /// <param name="collection">Collection.</param>
        /// <param name="func">Func.</param>
        /// <typeparam name="K">The 1st type parameter.</typeparam>
        /// <typeparam name="T">The 2nd type parameter.</typeparam>
        /*
        public static Dictionary<K, T> ToDictionary<K, T>(this IEnumerable<T> collection, Func<T, K> func)
        {
            var result = new Dictionary<K, T>();

            foreach (T obj in collection)
            {
                result.Add(func(obj), obj);
            }

            return result;
        }
        */

        /// <summary>
        /// Fors the each.
        /// </summary>
        /// <param name="enumeration">Enumeration.</param>
        /// <param name="action">Action.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static void ForEach<T>(this IEnumerable<T> enumeration, Action<T> action)
        {
            foreach (T item in enumeration)
            {
                action(item);
            }
        }
    }
}
