using System;
using System.Collections.Generic;

// ReSharper disable PossibleMultipleEnumeration

namespace ZenjectSignalHelpers.Utils
{
    internal static class EnumerableForEach
    {
        /// <summary>
        /// Call an action for each item in the enumerable.
        /// </summary>
        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            foreach (T item in enumerable)
                action(item);

            return enumerable;
        }
    }
}