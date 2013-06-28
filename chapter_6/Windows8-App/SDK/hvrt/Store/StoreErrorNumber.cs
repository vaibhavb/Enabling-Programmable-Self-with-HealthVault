using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HealthVault.Foundation;

namespace HealthVault.Store
{
    public enum StoreErrorNumber : uint
    {        
        ErrorBase = HResults.ErrorBase | 0x00004000,
        DuplicateChange,
        ItemAlreadyDeleted,
        TypeIDMismatch,
        ItemLocked,
        ItemNotLocked,
        ItemLockMismatch
    }

    internal class StoreException : Exception
    {
        public StoreException(StoreErrorNumber error)
            : this(error, null, null)
        {
        }

        public StoreException(StoreErrorNumber error, string message, Exception innerEx)
            : base(CreateErrorMessage(error, message), innerEx)
        {
            HResult = (int) error;
        }

        private static string CreateErrorMessage(StoreErrorNumber error, string message)
        {
            var builder = new StringBuilder();
            builder.AppendLine(error.ToString());
            if (!string.IsNullOrEmpty(message))
            {
                builder.AppendLine(message);
            }
            return builder.ToString();
        }
    }

}
