// (c) Microsoft. All rights reserved

using System;
using System.Xml.Serialization;
using HealthVault.Foundation;
using HealthVault.Types;

namespace HealthVault.ItemTypes
{
    public sealed class Immunization : IItemDataTyped
    {
        internal const string TypeIDString = "cd3587b5-b6e1-4565-ab3b-1c3ad45eb04f";
        internal const string RootElement = "immunization";
        private ItemProxy m_itemProxy;

        public Immunization()
        {
            Lot = String.Empty;
            AdverseEvent = String.Empty;
            Sequence = String.Empty;
            Consent = String.Empty;
            m_itemProxy = new ItemProxy(TypeID, this);
        }

        public Immunization(string name)
            : this()
        {
            Name = new CodableValue(name);
        }

        public static string TypeID
        {
            get { return TypeIDString; }
        }

        [XmlElement("name", Order = 1)]
        public CodableValue Name { get; set; }

        [XmlElement("administration-date", Order = 2)]
        public ApproxDateTime AdministrationDate { get; set; }

        [XmlElement("administrator", Order = 3)]
        public Person Administrator { get; set; }

        [XmlElement("manufacturer", Order = 4)]
        public CodableValue Manufacturer { get; set; }

        [XmlElement("lot", Order = 5)]
        public string Lot { get; set; }

        [XmlElement("route", Order = 6)]
        public CodableValue Route { get; set; }

        [XmlElement("expiration-date", Order = 7)]
        public ApproxDate ExpirationDate { get; set; }

        [XmlElement("sequence", Order = 8)]
        public string Sequence { get; set; }

        [XmlElement("anatomic-surface", Order = 9)]
        public CodableValue AnatomicSurface { get; set; }

        [XmlElement("adverse-event", Order = 10)]
        public string AdverseEvent { get; set; }

        [XmlElement("consent", Order = 11)]
        public string Consent { get; set; }

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
            Name.ValidateRequired("Name");
            AdministrationDate.ValidateOptional("AdministrationDate");
            Administrator.ValidateOptional("Administrator");
            Manufacturer.ValidateOptional("Manufacturer");
            Route.ValidateOptional("Route");
            ExpirationDate.ValidateOptional("ExpirationDate");
            AnatomicSurface.ValidateOptional("AnatomicSurface");
        }

        public HealthVault.Types.DateTime GetDateForEffectiveDate()
        {
            return (this.AdministrationDate != null) ? this.AdministrationDate.ToDateTime() : null;
        }

        #endregion

        public static Immunization Deserialize(string xml)
        {
            return HealthVaultClient.Serializer.FromXml<Immunization>(xml);
        }


        public static ItemQuery QueryFor()
        {
            return ItemQuery.QueryForTypeID(TypeID);
        }

        public static ItemFilter FilterFor()
        {
            return ItemFilter.FilterForType(TypeID);
        }

        public override string ToString()
        {
            return (Name != null) ? Name.ToString() : string.Empty;
        }

        public bool ShouldSerializeLot()
        {
            return !String.IsNullOrEmpty(Lot);
        }

        public bool ShouldSerializeAdverseEvent()
        {
            return !String.IsNullOrEmpty(AdverseEvent);
        }

        public bool ShouldSerializeSequence()
        {
            return !String.IsNullOrEmpty(Sequence);
        }

        public bool ShouldSerializeConsent()
        {
            return !String.IsNullOrEmpty(Consent);
        }

        //-----------------------------------------
        //
        // Vocabularies
        //
        //-----------------------------------------
        public static VocabIdentifier VocabForName()
        {
            return new VocabIdentifier(VocabFamily.HealthVault, VocabName.ImmunizationsCommon);
        }

        public static VocabIdentifier VocabForAnatomicSurface()
        {
            return new VocabIdentifier(VocabFamily.HealthVault, VocabName.ImmunizationAnatomicSurface);
        }

        public static VocabIdentifier VocabForAdverseEvent()
        {
            return new VocabIdentifier(VocabFamily.HealthVault, VocabName.ImmunizationAdverseEffect);
        }

        public static VocabIdentifier VocabForRoute()
        {
            return new VocabIdentifier(VocabFamily.HealthVault, VocabName.ImmunizationRoutes);
        }
    }
}