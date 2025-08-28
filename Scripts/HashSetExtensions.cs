using System;
using System.Collections.Generic;

namespace ShitSystem
{
    public static class HashSetExtensions
    {
        public static void Invoke<T>(this HashSet<T> set, Action<T> action)
        {
            if (set == null) throw new ArgumentNullException(nameof(set));
            if (action == null) throw new ArgumentNullException(nameof(action));

            
        }
    }
}