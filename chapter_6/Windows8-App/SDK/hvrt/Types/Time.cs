// (c) Microsoft. All rights reserved

using System;
using System.Xml.Serialization;
using HealthVault.Foundation;

namespace HealthVault.Types
{
    public sealed class Time : IHealthVaultTypeSerializable
    {
        public Time()
        {
        }

        public Time(DateTimeOffset dt)
            : this(dt.Hour, dt.Minute, dt.Second, dt.Millisecond)
        {
        }

        public Time(int hour, int minute, int second, int milliseconds)
        {
            if (hour >= 0)
            {
                Hour = new Hour(hour);
            }
            if (minute >= 0)
            {
                Minute = new Minute(minute);
            }
            if (second >= 0)
            {
                Second = new Second(second);
            }
            if (milliseconds >= 0)
            {
                Millisecond = new Millisecond(milliseconds);
            }
        }

        [XmlElement("h", Order = 1)]
        public Hour Hour { get; set; }

        [XmlElement("m", Order = 2)]
        public Minute Minute { get; set; }

        [XmlElement("s", Order = 3)]
        public Second Second { get; set; }

        [XmlElement("f", Order = 4)]
        public Millisecond Millisecond { get; set; }

        [XmlIgnore]
        public bool HasSecond
        {
            get { return (Second != null); }
        }

        [XmlIgnore]
        public bool HasMillisecond
        {
            get { return (Millisecond != null); }
        }

        #region IHealthVaultTypeSerializable Members

        public void Validate()
        {
            Hour.ValidateRequired("Hour");
            Minute.ValidateRequired("Minute");
            Second.ValidateOptional("Second");
            Millisecond.ValidateOptional("Millisecond");
        }

        public string Serialize()
        {
            return this.ToXml();
        }

        #endregion

        public TimeSpan ToTimeSpan()
        {
            if (HasSecond && HasMillisecond)
            {
                return new TimeSpan(
                    0,
                    Hour.ValueOrDefault(),
                    Minute.ValueOrDefault(),
                    Second.ValueOrDefault(),
                    Millisecond.ValueOrDefault());
            }

            if (HasSecond)
            {
                return new TimeSpan(Hour.Value, Minute.Value, Second.Value);
            }

            return new TimeSpan(Hour.ValueOrDefault(), Minute.ValueOrDefault(), 0);
        }

        public override string ToString()
        {
            return ToTimeSpan().ToString();
        }
    }
}