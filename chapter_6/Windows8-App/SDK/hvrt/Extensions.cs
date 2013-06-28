// (c) Microsoft. All rights reserved

using System;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;
using HealthVault.Foundation;
using Windows.UI.Core;
using Windows.Foundation;

namespace HealthVault
{
    internal static class Extensions
    {
        internal static T[] Append<T>(this T[] args, T item)
        {
            var copy = new T[args.Length + 1];
            args.CopyTo(copy, 0);
            copy[args.Length] = item;
            return copy;
        }

        internal static void AppendOptional(this StringBuilder builder, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                builder.Append(value);
            }
        }

        internal static void AppendOptional(this StringBuilder builder, string value, string separator)
        {
            if (!string.IsNullOrEmpty(value))
            {
                if (builder.Length > 0)
                {
                    builder.Append(separator);
                }

                builder.Append(value);
            }
        }

        internal static void SafeInvoke(this CompletionDelegate method, object sender, object result)
        {
            if (method != null)
            {
                try
                {
                    method(sender, result);
                }
                catch
                {
                }
            }
        }

        internal static void SafeInvoke<T>(this EventHandler<T> method, object sender, T args)
        {
            if (method != null)
            {
                try
                {
                    method(sender, args);
                }
                catch
                {
                }
            }
        }

        // Schedules a BACKGROUND operation to issue the notification.
        // The calling thread does not need to AWAIT
        internal static void SafeInvokeInUIThread<T>(this EventHandler<T> method, object sender, T args)
        {
            if (method == null)
            {
                return;
            }
            UIThreadDispatcher.Current.Schedule(() =>
            {
                SafeInvoke(method, sender, args);
            });
        }

        internal static void AddRange<T>(this HashSet<T> set, IEnumerable<T> values)
        {
            foreach(T value in values)
            {
                set.Add(value);
            }
        }
    }
}