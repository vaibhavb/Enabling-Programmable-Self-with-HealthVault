// (c) Microsoft. All rights reserved

using System;
using System.Xml.Serialization;
using HealthVault.Foundation;

namespace HealthVault.Types
{
    public sealed class StructuredDateTime : IHealthVaultTypeSerializable
    {
        public StructuredDateTime()
        {
        }

        public StructuredDateTime(DateTimeOffset dt)
        {
            Date = new Date(dt);
            Time = new Time(dt);
        }

        [XmlElement("date", Order = 1)]
        public Date Date { get; set; }

        [XmlElement("time", Order = 2)]
        public Time Time { get; set; }

        [XmlElement("tz", Order = 3)]
        public CodableValue Timezone { get; set; }

        [XmlIgnore]
        public bool HasTime
        {
            get { return (Time != null); }
        }

        [XmlIgnore]
        public bool HasTimezone
        {
            get { return (Timezone != null); }
        }

        #region IHealthVaultTypeSerializable Members

        public void Validate()
        {
            Date.ValidateRequired("Date");
        }

        public string Serialize()
        {
            return this.ToXml();
        }

        #endregion

        public DateTimeOffset ToDateTimeOffset()
        {
            if (!(Date != null || HasTime))
            {
                return default(DateTimeOffset);
            }

            if (!HasTime)
            {
                return Date.ToDateTime();
            }

            DateTimeOffset dt = Date.ToDateTime();
            TimeSpan ts = Time.ToTimeSpan();

            return dt.Add(ts);
        }
        
        public void FromDateTimeOffset(DateTimeOffset offset)
        {
            Date = new Date(offset);
            Time = new Time(offset);
        }

        internal DateTime ToDateTime()
        {
            return DateTime.FromSystemDateTime(this.ToDateTimeOffset());
        }

        public override string ToString()
        {
            return ToDateTimeOffset().ToString();
        }

        public static StructuredDateTime FromSystemDateTime(DateTimeOffset dt)
        {
            return new StructuredDateTime(dt);
        }

        public static StructuredDateTime FromString(string dateTimeText)
        {
            return new StructuredDateTime(DateTimeOffset.Parse(dateTimeText));
        }

        public static StructuredDateTime Now()
        {
            return new StructuredDateTime(DateTimeOffset.Now);
        }

        public static StructuredDateTime UtcNow()
        {
            return new StructuredDateTime(DateTimeOffset.UtcNow);
        }
    }
}