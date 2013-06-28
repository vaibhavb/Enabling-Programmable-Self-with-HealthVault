// (c) Microsoft. All rights reserved

using System;

namespace HealthVault.Types
{
    public interface IConstrainedDouble
    {
        double Min { get; }
        double Max { get; }
        double Value { get; set; }
        bool InRange { get; }
    }

    public static class ConstrainedDouble
    {
        public static double ValueOrDefault(this IConstrainedDouble val)
        {
            return (val != null) ? val.Value : default(double);
        }

        /// <summary>
        /// Inclusive check!
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static bool CheckRange(this IConstrainedDouble val)
        {
            double value = val.Value;
            return (value >= val.Min && value <= val.Max);
        }

        public static void Validate(this IConstrainedDouble val, string arg)
        {
            if (val == null || !val.CheckRange())
            {
                throw new ArgumentException(arg);
            }
        }
    }
}