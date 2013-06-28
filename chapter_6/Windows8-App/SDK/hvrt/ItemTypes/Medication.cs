// (c) Microsoft. All rights reserved

using System.Xml.Serialization;
using HealthVault.Foundation;
using HealthVault.Types;

namespace HealthVault.ItemTypes
{
    [XmlType(RootElement)]
    public sealed class Medication : IItemDataTyped
    {
        internal const string TypeIDString = "30cafccc-047d-4288-94ef-643571f7919d";
        internal const string RootElement = "medication";
        private ItemProxy m_itemProxy;

        public Medication()
        {
            m_itemProxy = new ItemProxy(TypeID, this);
        }

        public Medication(string name)
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

        [XmlElement("generic-name", Order = 2)]
        public CodableValue GenericName { get; set; }

        [XmlElement("dose", Order = 3)]
        public ApproxMeasurement Dose { get; set; }

        [XmlElement("strength", Order = 4)]
        public ApproxMeasurement Strength { get; set; }

        [XmlElement("frequency", Order = 5)]
        public ApproxMeasurement Frequency { get; set; }

        [XmlElement("route", Order = 6)]
        public CodableValue Route { get; set; }

        [XmlElement("indication", Order = 7)]
        public CodableValue Indication { get; set; }

        [XmlElement("date-started", Order = 8)]
        public ApproxDateTime StartDate { get; set; }

        [XmlElement("date-discontinued", Order = 9)]
        public ApproxDateTime StopDate { get; set; }

        [XmlElement("prescribed", Order = 10)]
        public CodableValue Prescribed { get; set; }

        [XmlElement("prescription", Order = 11)]
        public Prescription Prescription { get; set; }

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
            GenericName.ValidateOptional("GenericName");
            Dose.ValidateOptional("Dose");
            Strength.ValidateOptional("Strength");
            Frequency.ValidateOptional("Frequency");
            Route.ValidateOptional("Route");
            Indication.ValidateOptional("Indication");
            StartDate.ValidateOptional("StartDate");
            StopDate.ValidateOptional("StopDate");
            Prescribed.ValidateOptional("Prescribed");
            Prescription.ValidateOptional("Prescription");
        }

        public HealthVault.Types.DateTime GetDateForEffectiveDate()
        {
            return null;
        }

        #endregion

        public static Medication Deserialize(string xml)
        {
            return HealthVaultClient.Serializer.FromXml<Medication>(xml);
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

        //-----------------------------------------
        //
        // Vocabularies
        //
        //-----------------------------------------
        public static VocabIdentifier VocabForName()
        {
            return new VocabIdentifier(VocabFamily.RxNorm, VocabName.RxNormActiveMedicines);
        }

        public static VocabIdentifier VocabForDoseUnits()
        {
            return new VocabIdentifier(VocabFamily.HealthVault, VocabName.MedicationDoseUnits);
        }

        public static VocabIdentifier VocabForStrengthUnits()
        {
            return new VocabIdentifier(VocabFamily.HealthVault, VocabName.MedicationStrengthUnit);
        }

        public static VocabIdentifier VocabForRoute()
        {
            return new VocabIdentifier(VocabFamily.HealthVault, VocabName.MedicationRoutes);
        }

        public static VocabIdentifier VocabForIsPrescribed()
        {
            return new VocabIdentifier(VocabFamily.HealthVault, VocabName.MedicationPrescribed);
        }
    }
}