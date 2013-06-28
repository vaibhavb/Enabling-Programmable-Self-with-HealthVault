// (c) Microsoft. All rights reserved

using System;
using System.Xml.Serialization;
using HealthVault.Foundation;

namespace HealthVault.Types
{
    public sealed class ApproxDateTime : IHealthVaultTypeSerializable
    {
        public ApproxDateTime()
        {
            Description = String.Empty;
        }

        public ApproxDateTime(DateTimeOffset dt)
        {
            DateTime = new StructuredDateTime(dt);
        }

        //
        // The Elements below are a CHOICE - one or the other
        //
        [XmlElement("descriptive")]
        public string Description { get; set; }

        [XmlElement("structured")]
        public StructuredDateTime DateTime { get; set; }

        [XmlIgnore]
        public bool HasDateTime
        {
            get { return (DateTime != null); }
        }

        [XmlIgnore]
        public bool HasDescription
        {
            get { return (!string.IsNullOrEmpty(Description)); }
        }

        #region IHealthVaultTypeSerializable Members

        public void Validate()
        {
            // The data type is a choice... you can do one or the other
            if ((HasDescription && HasDateTime))
            {
                throw new ArgumentException("DateTime");
            }

            if (!(HasDescription || HasDateTime))
            {
                throw new ArgumentException("Description");
            }
        }

        public string Serialize()
        {
            return this.ToXml();
        }

        #endregion

        public override string ToString()
        {
            if (HasDescription)
            {
                return Description;
            }

            if (HasDateTime)
            {
                return DateTime.ToString();
            }

            return string.Empty;
        }

        public static ApproxDateTime Now()
        {
            return new ApproxDateTime
                   {
                       DateTime = StructuredDateTime.Now()
                   };
        }

        public bool ShouldSerializeDescription()
        {
            return !String.IsNullOrEmpty(Description);
        }

        internal DateTime ToDateTime()
        {
            if (this.HasDateTime)
            {
                return this.DateTime.ToDateTime();
            }

            return null;
        }
    }
}