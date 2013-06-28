// (c) Microsoft. All rights reserved

using System;
using System.Xml.Serialization;
using HealthVault.Foundation;

namespace HealthVault.Types
{
    public sealed class ExerciseSegment : IHealthVaultTypeSerializable
    {
        private StructuredNameValueCollection m_details;

        public ExerciseSegment()
        {
            Title = String.Empty;
        }

        [XmlElement("activity", Order = 1)]
        public CodableValue Activity { get; set; }

        [XmlElement("title", Order = 2)]
        public string Title { get; set; }

        [XmlElement("distance", Order = 3)]
        public LengthMeasurement Distance { get; set; }

        [XmlElement("duration", Order = 4)]
        public PositiveDouble Duration { get; set; }

        [XmlElement("offset", Order = 5)]
        public NonNegativeDouble Offset { get; set; }

        [XmlElement("detail", Order = 6)]
        public StructuredNameValueCollection Details
        {
            get
            {
                if (m_details == null)
                {
                    m_details = new StructuredNameValueCollection();
                }

                return m_details;
            }
            set { m_details = value; }
        }

        #region IHealthVaultTypeSerializable Members

        public string Serialize()
        {
            return this.ToXml();
        }

        public void Validate()
        {
            Activity.ValidateRequired("Activity");
            Distance.ValidateOptional("Distance");
            Duration.ValidateOptional("Duration");
            Offset.ValidateOptional("Offset");
            Details.ValidateOptional<StructuredNameValue>("Details");
        }

        #endregion

        public bool ShouldSerializeTitle()
        {
            return !String.IsNullOrEmpty(Title);
        }
    }
}