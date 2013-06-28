// (c) Microsoft. All rights reserved

using System;
using System.Xml.Serialization;
using HealthVault.Foundation;

namespace HealthVault.Types
{
    public sealed class ApproxDate : IHealthVaultTypeSerializable
    {
        public ApproxDate()
        {
        }

        public ApproxDate(DateTimeOffset dt)
        {
            Year = new Year(dt.Year);
            Month = new Month(dt.Month);
            Day = new Day(dt.Day);
        }

        [XmlElement("y", Order = 1)]
        public Year Year { get; set; }

        /// <summary>
        /// Optional
        /// </summary>
        [XmlElement("m", Order = 2)]
        public Month Month { get; set; }

        /// <summary>
        /// Optional
        /// </summary>
        [XmlElement("d", Order = 3)]
        public Day Day { get; set; }

        [XmlIgnore]
        public bool HasMonth
        {
            get { return Month != null; }
        }

        [XmlIgnore]
        public bool HasDay
        {
            get { return Day != null; }
        }

        #region IHealthVaultTypeSerializable Members

        public void Validate()
        {
            Year.ValidateRequired("Year");
            Month.ValidateOptional("Month");
            Day.ValidateOptional("Day");
        }

        public string Serialize()
        {
            return this.ToXml();
        }

        #endregion
    }
}