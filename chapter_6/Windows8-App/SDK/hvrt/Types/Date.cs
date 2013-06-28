// (c) Microsoft. All rights reserved

using System;
using System.Xml.Serialization;
using HealthVault.Foundation;

namespace HealthVault.Types
{
    public sealed class Date : IHealthVaultTypeSerializable
    {
        public Date()
        {
        }

        public Date(DateTimeOffset date)
            : this(date.Year, date.Month, date.Day)
        {
        }

        public Date(int year, int month, int day)
        {
            if (year >= 0)
            {
                Year = new Year(year);
            }
            if (month >= 0)
            {
                Month = new Month(month);
            }
            if (day >= 0)
            {
                Day = new Day(day);
            }
        }

        [XmlElement("y", Order = 1)]
        public Year Year { get; set; }

        [XmlElement("m", Order = 2)]
        public Month Month { get; set; }

        [XmlElement("d", Order = 3)]
        public Day Day { get; set; }

        #region IHealthVaultTypeSerializable Members

        public string Serialize()
        {
            return this.ToXml();
        }

        public void Validate()
        {
            Year.ValidateRequired("Year");
            Month.ValidateRequired("Month");
            Day.ValidateRequired("Day");
        }

        #endregion

        public static Date Now()
        {
            return new Date(DateTimeOffset.Now);
        }

        public static Date UtcNow()
        {
            return new Date(DateTimeOffset.UtcNow);
        }

        public DateTimeOffset ToDateTime()
        {
            int month = Month == null ? 1 : Month.Value;
            int day = Day == null ? 1 : Day.Value;

            return new DateTimeOffset(
                Year.ValueOrDefault(), month, day, 0, 0, 0, TimeZoneInfo.Local.BaseUtcOffset);
        }

        public override string ToString()
        {
            return (ToDateTime()).ToString("d");
        }
    }
}