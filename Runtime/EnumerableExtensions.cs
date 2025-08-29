using System;
using System.Collections.Generic;

namespace ShitSystem
{
    public static class EnumerableExtensions
    {
        public static void Invoke<T>(this IEnumerable<T> set, Action<T> action)
        {
            if (set == null) throw new ArgumentNullException(nameof(set));
            if (action == null) throw new ArgumentNullException(nameof(action));

            foreach (var item in set)
            {
                action(item);
            }
        }
    }
}