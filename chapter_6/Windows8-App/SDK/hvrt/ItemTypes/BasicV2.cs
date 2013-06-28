// (c) Microsoft. All rights reserved

using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using HealthVault.Foundation;
using HealthVault.Types;

namespace HealthVault.ItemTypes
{
    [XmlType(RootElement)]
    public sealed class BasicV2 : IItemDataTyped
    {
        internal const string TypeIDString = "3b3e6b16-eb69-483c-8d7e-dfe116ae6092";
        internal const string RootElement = "basic";
        private ItemProxy m_itemProxy;

        public BasicV2()
        {
            Gender = String.Empty;
            PostalCode = String.Empty;
            City = String.Empty;
            m_itemProxy = new ItemProxy(TypeID, this);
        }

        public static string TypeID
        {
            get { return TypeIDString; }
        }

        [XmlElement("gender", Order = 1)]
        public string Gender { get; set; }

        [XmlElement("birthyear", Order = 2)]
        public Year BirthYear { get; set; }

        [XmlElement("country", Order = 3)]
        public CodableValue Country { get; set; }

        [XmlElement("postcode", Order = 4)]
        public string PostalCode { get; set; }

        [XmlElement("city", Order = 5)]
        public string City { get; set; }

        [XmlElement("state", Order = 6)]
        public CodableValue State { get; set; }

        [XmlElement("firstdow", Order = 7)]
        public Types.DayOfWeek FirstDayOfWeek { get; set; }

        [XmlElement("language", Order = 8)]
        public Language[] Languages { get; set; }

        #region IItemDataTyped Members

        [XmlIgnore]
        public ItemType Type
        {
            get { return m_itemProxy.Item.Type; }
        }

        [XmlIgnore]
        public ItemKey Key
        {
            get { return m_itemProxy.Item.Key; }
            set { m_itemProxy.Item.Key = value; }
        }

        [XmlIgnore]
        public RecordItem Item
        {
            get { return m_itemProxy; }
        }

        [XmlIgnore]
        public ItemData ItemData
        {
            get { return m_itemProxy.ItemData; }
            set { m_itemProxy.ItemData = value; }
        }

        public string Serialize()
        {
            return this.ToXml();
        }

        public void Validate()
        {
            if (!String.IsNullOrEmpty(Gender) && Gender != "m" && Gender != "f")
            {
                throw new ArgumentException("Gender");
            }

            BirthYear.ValidateOptional("BirthYear");
            Country.ValidateOptional("Country");
            State.ValidateOptional("State");
            FirstDayOfWeek.ValidateOptional("FirstDayOfWeek");
            Languages.ValidateOptional<Language>("Languages");
        }

        public HealthVault.Types.DateTime GetDateForEffectiveDate()
        {
            return null;
        }

        #endregion

        public static BasicV2 Deserialize(string xml)
        {
            return HealthVaultClient.Serializer.FromXml<BasicV2>(xml);
        }

        public static ItemQuery QueryFor()
        {
            return ItemQuery.QueryForTypeID(TypeID);
        }

        public static ItemFilter FilterFor()
        {
            return ItemFilter.FilterForType(TypeID);
        }
        
        public bool ShouldSerializeGender()
        {
            return !String.IsNullOrEmpty(Gender);
        }

        public bool ShouldSerializePostalCode()
        {
            return !String.IsNullOrEmpty(PostalCode);
        }

        public bool ShouldSerializeCity()
        {
            return !String.IsNullOrEmpty(City);
        }

        //-----------------------------------------
        //
        // Vocabularies
        //
        //-----------------------------------------
        public static VocabIdentifier VocabForCountry()
        {
            return new VocabIdentifier(VocabFamily.ISO, VocabName.Country);
        }
    }
}