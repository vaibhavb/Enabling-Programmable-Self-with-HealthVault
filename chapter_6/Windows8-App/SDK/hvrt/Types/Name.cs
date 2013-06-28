// (c) Microsoft. All rights reserved

using System;
using System.Text;
using System.Xml.Serialization;
using HealthVault.Foundation;

namespace HealthVault.Types
{
    public sealed class Name : IHealthVaultTypeSerializable
    {
        public Name()
        {
            Full = String.Empty;
            First = String.Empty;
            Middle = String.Empty;
            Last = String.Empty;
        }

        public Name(string name) : this()
        {
            Full = name;
        }

        public Name(string first, string last)
            : this(first, null, last)
        {
        }

        public Name(string first, string middle, string last) : this()
        {
            First = first;
            Middle = middle;
            Last = last;

            Full = BuildFullName();
        }

        [XmlElement("full", Order = 1)]
        public string Full { get; set; }

        [XmlElement("title", Order = 2)]
        public CodableValue Title { get; set; }

        [XmlElement("first", Order = 3)]
        public string First { get; set; }

        [XmlElement("middle", Order = 4)]
        public string Middle { get; set; }

        [XmlElement("last", Order = 5)]
        public string Last { get; set; }

        [XmlElement("suffix", Order = 6)]
        public CodableValue Suffix { get; set; }

        #region IHealthVaultTypeSerializable Members

        public string Serialize()
        {
            return this.ToXml();
        }

        public void Validate()
        {
            Full.ValidateRequired("Full");
        }

        #endregion

        public string BuildFullName()
        {
            var fullName = new StringBuilder();
            if (Title != null)
            {
                fullName.AppendOptional(Title.Text);
            }

            fullName.AppendOptional(First, " ");

            if (!string.IsNullOrEmpty(Middle))
            {
                string middleInitial = string.Format("{0}.", char.ToUpper(Middle[0]));
                fullName.AppendOptional(middleInitial, " ");
            }

            fullName.AppendOptional(Last, " ");
            if (Suffix != null)
            {
                fullName.AppendOptional(Suffix.Text, " ");
            }

            return fullName.ToString();
        }

        public override string ToString()
        {
            return (Full != null) ? Full : string.Empty;
        }

        public bool ShouldSerializeFull()
        {
            return !String.IsNullOrEmpty(Full);
        }

        public bool ShouldSerializeFirst()
        {
            return !String.IsNullOrEmpty(First);
        }

        public bool ShouldSerializeMiddle()
        {
            return !String.IsNullOrEmpty(Middle);
        }

        public bool ShouldSerializeLast()
        {
            return !String.IsNullOrEmpty(Last);
        }
    }
}