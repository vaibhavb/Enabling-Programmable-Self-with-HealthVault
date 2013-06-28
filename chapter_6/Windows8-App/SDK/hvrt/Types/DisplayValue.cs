// (c) Microsoft. All rights reserved

using System;
using System.Xml.Serialization;
using HealthVault.Foundation;

namespace HealthVault.Types
{
    public sealed class DisplayValue : IHealthVaultTypeSerializable
    {
        public DisplayValue()
        {
            Units = String.Empty;
            Code = String.Empty;
            Text = String.Empty;
        }

        public DisplayValue(double numValue, string units)
            : this(numValue, units, null)
        {
        }

        public DisplayValue(double numValue, string units, string unitsCode)
        {
            if (string.IsNullOrEmpty(units))
            {
                throw new ArgumentException("units");
            }
            Value = numValue;
            Units = units;
            Code = unitsCode;
        }

        [XmlAttribute("units")]
        public string Units { get; set; }

        [XmlAttribute("units-code")]
        public string Code { get; set; }

        [XmlAttribute("text")]
        public string Text { get; set; }

        [XmlText]
        public double Value { get; set; }

        [XmlIgnore]
        public bool HasText
        {
            get { return !string.IsNullOrEmpty(Text); }
        }

        [XmlIgnore]
        public bool HasUnits
        {
            get { return !string.IsNullOrEmpty(Units); }
        }

        [XmlIgnore]
        public bool HasCode
        {
            get { return !string.IsNullOrEmpty(Code); }
        }

        #region IHealthVaultTypeSerializable Members

        public string Serialize()
        {
            return this.ToXml();
        }

        void IHealthVaultType.Validate()
        {
            Units.ValidateRequired("Units");
        }

        #endregion

        public bool ShouldSerializeUnits()
        {
            return !String.IsNullOrEmpty(Units);
        }

        public bool ShouldSerializeCode()
        {
            return !String.IsNullOrEmpty(Code);
        }

        public bool ShouldSerializeText()
        {
            return !String.IsNullOrEmpty(Text);
        }
    }
}