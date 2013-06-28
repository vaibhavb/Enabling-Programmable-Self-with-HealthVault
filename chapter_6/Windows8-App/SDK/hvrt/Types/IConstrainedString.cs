// (c) Microsoft. All rights reserved

using System;

namespace HealthVault.Types
{
    public interface IConstrainedString
    {
        int Length { get; }
        int MinLength { get; }
        int MaxLength { get; }
        string Value { get; set; }

        bool InRange { get; }
    }

    public static class ConstrainedString
    {
        public static bool CheckRange(this IConstrainedString val)
        {
            int length = val.Length;
            return (length >= val.MinLength && length <= val.MaxLength);
        }

        public static void Validate(this IConstrainedString val, string arg)
        {
            if (val == null || !val.CheckRange())
            {
                throw new ArgumentException(arg);
            }
        }
    }

    /// <summary>
    /// To get around WinRT inheritance restrictions
    /// </summary>
    internal struct ConstrainedStringImpl
    {
        private readonly int m_max;
        private readonly int m_min;
        private string m_value;

        internal ConstrainedStringImpl(int min, int max)
        {
            m_min = min;
            m_max = max;
            m_value = null;
        }

        public int Length
        {
            get { return (m_value != null) ? m_value.Length : 0; }
        }

        public int MinLength
        {
            get { return m_min; }
        }

        public int MaxLength
        {
            get { return m_max; }
        }

        public string Value
        {
            get { return m_value; }
            set { m_value = value; }
        }

        public bool InRange
        {
            get
            {
                int length = Length;
                return (length >= m_min && length <= m_max);
            }
        }

        public static implicit operator string(ConstrainedStringImpl value)
        {
            return value.Value;
        }
    }
}