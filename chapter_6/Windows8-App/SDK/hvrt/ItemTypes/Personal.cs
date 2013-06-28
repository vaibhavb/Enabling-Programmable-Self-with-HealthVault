// (c) Microsoft. All rights reserved

using System;
using System.Xml.Serialization;
using HealthVault.Foundation;
using HealthVault.Types;

namespace HealthVault.ItemTypes
{
    [XmlType(RootElement)]
    public sealed class Personal : IItemDataTyped
    {
        internal const string TypeIDString = "92ba621e-66b3-4a01-bd73-74844aed4f5b";
        internal const string RootElement = "personal";
        private ItemProxy m_itemProxy;

        public Personal()
        {
            NationalIdentifier = String.Empty;
            EmploymentStatus = String.Empty;
            OrganDonor = String.Empty;
            m_itemProxy = new ItemProxy(TypeID, this);
        }

        public static string TypeID
        {
            get { return TypeIDString; }
        }

        [XmlElement("name", Order = 1)]
        public Name Name { get; set; }

        [XmlElement("birthdate", Order = 2)]
        public StructuredDateTime BirthDate { get; set; }

        [XmlElement("blood-type", Order = 3)]
        public CodableValue BloodType { get; set; }

        [XmlElement("ethnicity", Order = 4)]
        public CodableValue Ethnicity { get; set; }

        [XmlElement("ssn", Order = 5)]
        public string NationalIdentifier { get; set; }

        [XmlElement("marital-status", Order = 6)]
        public CodableValue MaritalStatus { get; set; }

        [XmlElement("employment-status", Order = 7)]
        public string EmploymentStatus { get; set; }

        [XmlElement("is-deceased", Order = 8)]
        public BooleanValue IsDeceased { get; set; }

        [XmlElement("date-of-death", Order = 9)]
        public ApproxDateTime DateOfDeath { get; set; }

        [XmlElement("religion", Order = 10)]
        public CodableValue Religion { get; set; }

        [XmlElement("is-veteran", Order = 11)]
        public BooleanValue IsVeteran { get; set; }

        [XmlElement("highest-education-level", Order = 12)]
        public CodableValue EducationLevel { get; set; }

        [XmlElement("is-disabled", Order = 13)]
        public BooleanValue IsDisabled { get; set; }

        [XmlElement("organ-donor", Order = 14)]
        public string OrganDonor { get; set; }

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
            Name.ValidateOptional("Name");
            BirthDate.ValidateOptional("BirthDate");
            BloodType.ValidateOptional("BloodType");
            Ethnicity.ValidateOptional("Ethnicity");
            MaritalStatus.ValidateOptional("MaritalStatus");
            IsDeceased.ValidateOptional("IsDeceased");
            DateOfDeath.ValidateOptional("DateOfDeath");
            Religion.ValidateOptional("Religion");
            IsVeteran.ValidateOptional("IsVeteran");
            EducationLevel.ValidateOptional("EducationLevel");
            IsDisabled.ValidateOptional("IsDisabled");
        }

        public HealthVault.Types.DateTime GetDateForEffectiveDate()
        {
            return null;
        }

        #endregion

        public static Personal Deserialize(string xml)
        {
            return HealthVaultClient.Serializer.FromXml<Personal>(xml);
        }

        public static ItemQuery QueryFor()
        {
            return ItemQuery.QueryForTypeID(TypeID);
        }

        public static ItemFilter FilterFor()
        {
            return ItemFilter.FilterForType(TypeID);
        }

        public bool ShouldSerializeNationalIdentifier()
        {
            return !String.IsNullOrEmpty(NationalIdentifier);
        }

        public bool ShouldSerializeEmploymentStatus()
        {
            return !String.IsNullOrEmpty(EmploymentStatus);
        }

        public bool ShouldSerializeOrganDonor()
        {
            return !String.IsNullOrEmpty(OrganDonor);
        }

        //-----------------------------------------
        //
        // Vocabularies
        //
        //-----------------------------------------
        public static VocabIdentifier VocabForBloodType()
        {
            return new VocabIdentifier(VocabFamily.HealthVault, VocabName.BloodType);
        }

        public static VocabIdentifier VocabForEthnicity()
        {
            return new VocabIdentifier(VocabFamily.HealthVault, VocabName.Ethnicity);
        }

        public static VocabIdentifier VocabForMaritalStatus()
        {
            return new VocabIdentifier(VocabFamily.HealthVault, VocabName.MaritalStatus);
        }

        public static VocabIdentifier VocabForReligion()
        {
            return new VocabIdentifier(VocabFamily.HealthVault, VocabName.Religion);
        }

        public static VocabIdentifier VocabForEducationLevel()
        {
            return new VocabIdentifier(VocabFamily.HealthVault, VocabName.EducationLevel);
        }
    }
}