// (c) Microsoft. All rights reserved

using System;
using System.Xml;
using System.Xml.Serialization;

namespace HealthVault.Types
{
    public sealed class DateTime : IHealthVaultType
    {
        public DateTime()
        {
        }

        public DateTime(string textDate)
        {
            Value = DateTimeOffset.Parse(textDate);
        }

        //
        // Can't serialize this directly
        //
        [XmlIgnore]
        public DateTimeOffset Value { get; set; }

        [XmlText]
        public string TextValue
        {
            get 
            { 
                string xml = XmlConvert.ToString(Value); 
                return xml;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentException("TextValue");
                }
                Value = XmlConvert.ToDateTimeOffset(value);
             }
        }

        #region IHealthVaultType Members

        public void Validate()
        {
        }

        #endregion

        public override bool Equals(object obj)
        {
            var other = obj as DateTime;
            if (other == null)
            {
                return base.Equals(obj);
            }

            return EqualsDateTime(other);
        }

        public bool EqualsDateTime(DateTime other)
        {
            if (other == null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return (Value.Equals(other.Value));
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static int Compare(DateTime dt1, DateTime dt2)
        {
            if (dt1 == null)
            {
                if (dt2 == null)
                {
                    return 0;
                }

                return -1;
            }

            if (dt2 == null)
            {
                return 1;
            }

            return dt1.Value.CompareTo(dt2.Value);
        }

        public override string ToString()
        {
            return TextValue;
        }

        public static DateTime FromSystemDateTime(DateTimeOffset dt)
        {
            return new DateTime
                   {
                       Value = dt
                   };
        }

        public static Date FromSystemDate(DateTimeOffset dt)
        {
            return new Date(dt);
        }

        public static Date FromString(string dateTimeText)
        {
            return new Date(DateTimeOffset.Parse(dateTimeText));
        }

        public static DateTime Now()
        {
            return FromSystemDateTime(DateTimeOffset.Now);
        }

        public static DateTime UtcNow()
        {
            return FromSystemDateTime(DateTimeOffset.UtcNow);
        }
    }
}