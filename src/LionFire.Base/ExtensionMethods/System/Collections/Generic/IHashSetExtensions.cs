﻿#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.ExtensionMethods
{
    public static class IHashSetExtensions
    {
        public static void TryAdd<T>(this HashSet<T> set, T obj)
        {
            if (!set.Contains(obj)) set.Add(obj);
        }

        public static void AddRange<T>(this HashSet<T> hashSet, IEnumerable<T> enumerable)
        {
            foreach(var e in enumerable)
            {
                hashSet.Add(e);
            }
        }
    }
}
