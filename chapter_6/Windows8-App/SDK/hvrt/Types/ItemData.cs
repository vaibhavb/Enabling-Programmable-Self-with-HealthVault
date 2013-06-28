// (c) Microsoft. All rights reserved

using System;
using System.Xml.Serialization;
using HealthVault.Foundation;
using HealthVault.ItemTypes;

namespace HealthVault.Types
{
    public sealed class ItemData : IHealthVaultTypeSerializable
    {
        private ItemDataCommon m_common;
        private IItemDataTyped m_typedData;

        public ItemData()
        {
        }

        public ItemData(IItemDataTyped typedData)
            : this(typedData, null)
        {
        }

        public ItemData(IItemDataTyped typedData, ItemDataCommon commonData)
        {
            if (typedData == null)
            {
                throw new ArgumentNullException("typedData");
            }
            Typed = typedData;
            Common = commonData;
        }

        [XmlElement(Allergy.RootElement, typeof(Allergy))]
        [XmlElement(BasicV2.RootElement, typeof(BasicV2))]
        [XmlElement(BloodGlucose.RootElement, typeof(BloodGlucose))]
        [XmlElement(BloodPressure.RootElement, typeof(BloodPressure))]
        [XmlElement(Cholesterol.RootElement, typeof (Cholesterol))]
        [XmlElement(Condition.RootElement, typeof(Condition))]
        [XmlElement(ItemTypes.Contact.RootElement, typeof(ItemTypes.Contact))]
        [XmlElement(DietaryIntake.RootElement, typeof (DietaryIntake))]
        [XmlElement(Exercise.RootElement, typeof (Exercise))]
        [XmlElement(File.RootElement, typeof(File))]
        [XmlElement(HealthGoal.RootElement, typeof(HealthGoal))]
        [XmlElement(Height.RootElement, typeof (Height))]
        [XmlElement(Immunization.RootElement, typeof(Immunization))]
        [XmlElement(Insurance.RootElement, typeof(Insurance))]
        [XmlElement(Medication.RootElement, typeof(Medication))]
        [XmlElement(Personal.RootElement, typeof(Personal))]
        [XmlElement(PersonalImage.RootElement, typeof (PersonalImage))]
        [XmlElement(Procedure.RootElement, typeof (Procedure))]
        [XmlElement(Weight.RootElement, typeof (Weight))]
        [XmlElement(MealDefinition.RootElement, typeof(MealDefinition))]
        public object Typed // Must be object. Can't serialize interfaces. 
        {
            get { return m_typedData; }
            set
            {
                if (value == null)
                {
                    m_typedData = null;
                    return;
                }

                var typedData = value as IItemDataTyped;
                if (typedData == null)
                {
                    throw new ArgumentException("itemData");
                }
                m_typedData = typedData;
                m_typedData.ItemData = this;
            }
        }

        [XmlElement("common")]
        public ItemDataCommon Common
        {
            get
            {
                if (m_common == null)
                {
                    m_common = new ItemDataCommon();
                }
                return m_common;
            }
            set { m_common = value; }
        }

        [XmlIgnore]
        public bool HasCommon
        {
            get { return (Common != null); }
        }

        [XmlIgnore]
        public bool HasTyped
        {
            get { return (Typed != null); }
        }

        /// <summary>
        /// The RecordItem this ItemData belongs to
        /// </summary>
        [XmlIgnore]
        public RecordItem Item { get; set; }

        #region IHealthVaultTypeSerializable Members

        public string Serialize()
        {
            return this.ToXml();
        }

        public void Validate()
        {
            if (HasTyped)
            {
                ((IItemDataTyped) Typed).ValidateRequired("Typed");
            }
            if (HasCommon)
            {
                Common.ValidateOptional("Common");
            }
        }

        #endregion
    }
}