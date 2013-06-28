// (c) Microsoft. All rights reserved

using System;
using System.Xml.Serialization;
using HealthVault.Foundation;

namespace HealthVault.Types
{
    public sealed class GoalRecurrence : IHealthVaultTypeSerializable
    {
        [XmlElement("interval", Order = 1)]
        public CodableValue Interval { get; set; }

        [XmlElement("times-in-interval", Order = 2)]
        public int TimesInInterval { get; set; }

        #region IHealthVaultTypeSerializable Members

        public string Serialize()
        {
            return this.ToXml();
        }

        public void Validate()
        {
            Interval.ValidateRequired("Interval");
            TimesInInterval.ValidateRequired("TimesInInterval");
        }

        public override string ToString()
        {
            return String.Format("{0} / {1}", TimesInInterval, Interval);
        }

        #endregion
    }
}
