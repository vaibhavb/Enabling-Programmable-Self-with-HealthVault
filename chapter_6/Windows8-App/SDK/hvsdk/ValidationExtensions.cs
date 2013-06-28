// (c) Microsoft. All rights reserved
using System;

namespace HealthVault.Foundation
{
    public static class ValidationExtensions
    {
        public static void ValidateRequired(this IValidatable item, string arg)
        {
            if (item == null)
            {
                throw new ArgumentNullException(arg);
            }
            item.Validate();
        }

        public static void ValidateOptional(this IValidatable item)
        {
            if (item == null)
            {
                return;
            }

            item.Validate();
        }

        public static void ValidateRequired(this string item, string arg)
        {
            if (string.IsNullOrEmpty(item))
            {
                throw new ArgumentException(arg);
            }
        }

        public static void ValidateRequired(this object item, string arg)
        {
            if (item == null)
            {
                throw new ArgumentNullException(arg);
            }
        }

        public static void ValidateRequired(this Guid item, string arg)
        {
            if (item.IsEmpty())
            {
                throw new ArgumentException(arg);
            }
        }

        public static void Validate(this int value, int min, int max, string arg)
        {
            if (value < min || value > max)
            {
                throw new ArgumentException(arg);
            }
        }

        public static void Validate(this double value, double min, double max, string arg)
        {
            if (value < min || value > max)
            {
                throw new ArgumentException(arg);
            }
        }
    }
}