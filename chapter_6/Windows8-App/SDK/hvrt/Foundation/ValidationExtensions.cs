using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HealthVault.Foundation;

namespace HealthVault.Foundation
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

        public static void ValidateOptional(this IHealthVaultType item)
        {
            if (item == null)
            {
                return;
            }

            item.Validate();
        }

        public static void ValidateRequired(this Array array, string arg)
        {
            if (array == null)
            {
                throw new ArgumentNullException(arg);
            }

            foreach(object obj in array)
            {
                IHealthVaultType hvType = obj as IHealthVaultType;
                if (hvType != null)
                {
                    hvType.Validate();
                } 
                else
                {
                    obj.ValidateRequired(arg);
                }
            }
        }
    }
}
