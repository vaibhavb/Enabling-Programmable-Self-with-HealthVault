// (c) Microsoft. All rights reserved
using System;
using System.Collections.Generic;
using Windows.Foundation.Collections;

namespace HealthVault.Foundation
{
    public static class Extensions
    {
        public static bool IsNullOrEmpty<T>(this IList<T> array)
        {
            return (array == null || array.Count == 0);
        }

        public static bool ContainsNullItems(this Array array)
        {
            foreach (object obj in array)
            {
                if (obj == null)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsEmpty(this Guid guid)
        {
            return (guid == Guid.Empty);
        }

        public static bool SafeEquals(this string src, string other)
        {
            if (src == null && other == null)
            {
                return true;
            }

            if (src == null || other == null)
            {
                return false;
            }

            return (src.Length == other.Length && src.Equals(other));
        }

        public static bool SafeEquals(this string src, string other, StringComparison comparison)
        {
            return string.Equals(src, other, comparison);
        }

        public static void SafeInvoke(this Action action)
        {
            if (action == null)
            {
                return;
            }
            try
            {
                action();
            }
            catch
            {
            }
        }

        public static void SafeInvokeEvent<T>(this EventHandler<T> handler, object sender, T value)
        {
            if (handler == null)
            {
                return;
            }
            try
            {
                handler(sender, value);
            }
            catch
            {
            }
        }

        public static void SafeInvoke<T>(this Action<T> action, T value)
        {
            if (action == null)
            {
                return;
            }
            try
            {
                action(value);
            }
            catch
            {
            }
        }

        public static void SafeInvoke<T1, T2>(this Action<T1, T2> action, T1 t1, T2 t2)
        {
            if (action == null)
            {
                return;
            }
            try
            {
                action(t1, t2);
            }
            catch
            {
            }
        }

        public static void Put(this IPropertySet set, string key, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                set.Remove(key);
            }
            else
            {
                set[key] = value;
            }
        }
    }
}