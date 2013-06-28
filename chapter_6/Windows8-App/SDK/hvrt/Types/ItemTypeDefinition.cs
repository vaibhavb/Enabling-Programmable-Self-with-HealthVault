// (c) Microsoft. All rights reserved

using System.Xml.Serialization;
using HealthVault.Foundation;
using HealthVault.Types;

namespace HealthVault.Types
{
    public sealed class ItemTypeDefinition : IHealthVaultTypeSerializable
    {
        [XmlElement("id", Order = 0)]
        public string TypeId { get; set; }

        [XmlElement("name", Order = 1)]
        public string Name { get; set; }

        //
        // Core Section
        //
        [XmlElement("uncreatable", Order = 2)]
        public BooleanValue Uncreatable { get; set; }

        [XmlElement("immutable", Order = 3)]
        public BooleanValue Immutable { get; set; }

        [XmlElement("singleton", Order = 4)]
        public BooleanValue Singleton { get; set; }

        //
        // Xsd Section
        //
        [XmlElement("xsd", Order = 5)]
        public string Xsd { get; set; }

        /* TODO: Implement these additional properties that we currently don't use
        //
        // Columns Section
        //
        [XmlElement("columns", Order = 6)]
        public Columns Columns { get; set; }

        //
        // Transforms Section
        //
        [XmlElement("transforms", Order = 7)]
        public Transforms Transforms { get; set; }

        //
        // Transform Source Section
        //
        [XmlElement("transform-source", Order = 8)]
        public TransformSourceCollection TransformSources { get; set; }

        //
        // Images Section 
        //
        [XmlElement("image", Order = 9)]
        public ImageCollection Images { get; set; }

        */

        //
        // Version Section
        //
        [XmlElement("versions", Order = 10)]
        public Versions Versions { get; set; }

        [XmlElement("effective-date-xpath", Order = 11)]
        public string EffectiveDateXPath { get; set; }

        #region IHealthVaultTypeSerializable Members

        public void Validate()
        {
            TypeId.ValidateRequired("TypeId");
            Name.ValidateRequired("Name");
            Uncreatable.ValidateOptional("Uncreatable");
            Immutable.ValidateOptional("Immutable");
            Singleton.ValidateOptional("Singleton");
            Versions.ValidateOptional("Versions");
        }

        public string Serialize()
        {
            return this.ToXml();
        }

        #endregion
    }
}