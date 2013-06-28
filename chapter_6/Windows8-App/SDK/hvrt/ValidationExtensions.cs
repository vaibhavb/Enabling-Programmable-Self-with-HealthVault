// (c) Microsoft. All rights reserved

using System;
using System.Collections.Generic;
using HealthVault.Types;

namespace HealthVault
{
    internal static class ValidationExtensions
    {
        public static void ValidateRequired(this IHealthVaultType item, string arg)
        {
            if (item == null)
            {
                throw new ArgumentNullException(arg);
            }
            item.Validate();
        }

        public static void ValidateOptional(this IHealthVaultType item, string arg)
        {
            if (item == null)
            {
                return;
            }

            item.ValidateRequired(arg);
        }

        public static void ValidateRequired<T>(this IList<T> array, string arg)
        {
            if (array == null)
            {
                throw new ArgumentNullException(arg);
            }

            array.ValidateOptional(arg);
        }

        public static void ValidateOptional<T>(this IList<T> array, string arg)
        {
            if (array == null)
            {
                return;
            }

            foreach (object item in array)
            {
                var validator = item as IHealthVaultType;
                if (validator != null)
                {
                    validator.Validate();
                }
            }
        }
    }
}