// (c) Microsoft. All rights reserved

using System;

namespace HealthVault.Types
{
    public interface IConstrainedInt
    {
        int Min { get; }
        int Max { get; }
        int Value { get; set; }

        bool InRange { get; }
    }

    public static class ConstrainedInt
    {
        public static int ValueOrDefault(this IConstrainedInt val)
        {
            return (val != null) ? val.Value : default(int);
        }

        public static bool CheckRange(this IConstrainedInt val)
        {
            int value = val.Value;
            return (value >= val.Min && value <= val.Max);
        }

        public static void Validate(this IConstrainedInt val, string arg)
        {
            if (val == null || !val.CheckRange())
            {
                throw new ArgumentException(arg);
            }
        }
    }
}