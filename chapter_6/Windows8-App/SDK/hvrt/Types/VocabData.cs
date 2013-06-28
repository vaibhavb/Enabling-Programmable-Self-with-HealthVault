// (c) Microsoft. All rights reserved

using System;
using System.Xml.Serialization;
using HealthVault.Foundation;
using HealthVault.ItemTypes;

namespace HealthVault.Types
{
    public sealed class VocabData : IHealthVaultTypeSerializable
    {
        public VocabData()
        {
        }

        public VocabData(IHealthVaultType typedData)
        {
            if (typedData == null)
            {
                throw new ArgumentNullException("typedData");
            }

            Typed = typedData;
        }

        [XmlElement(NutritionInformation.RootElement, typeof(NutritionInformation))]
        [XmlElement(UnitConversions.RootElement, typeof(UnitConversions))]
        public object Typed { get; set; }

        [XmlIgnore]
        public bool HasTyped
        {
            get { return (Typed != null); }
        }
        
        #region IHealthVaultTypeSerializable Members

        public string Serialize()
        {
            return this.ToXml();
        }

        public void Validate()
        {
            if (HasTyped)
            {
                ((IHealthVaultType)Typed).ValidateRequired("Typed");
            }
        }

        #endregion
    }
}