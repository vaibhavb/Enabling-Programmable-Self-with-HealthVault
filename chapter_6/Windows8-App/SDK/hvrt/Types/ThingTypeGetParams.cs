using System;
using System.Xml.Serialization;
using HealthVault.Foundation;

namespace HealthVault.Types
{
    [Flags]
    public enum ThingTypeSectionType
    {
        None = 0,
        Core = 0x01,
        Xsd = 0x02,
        Columns = 0x04,
        Transforms = 0x08,
        TransformSource = 0x10,
        Versions = 0x20,

        Standard = Core | Versions
    }

    public sealed class ThingTypeSections
    {
        public static string Core
        {
            get { return "core"; }
        }

        public static string Xsd
        {
            get { return "xsd"; }
        }

        public static string Columns
        {
            get { return "columns"; }
        }

        public static string Transforms
        {
            get { return "transforms"; }
        }

        public static string TransformSource
        {
            get { return "transformsource"; }
        }

        public static string Versions
        {
            get { return "versions"; }
        }
    }

    public sealed class ThingTypeGetParams : IHealthVaultTypeSerializable
    {
        private LazyField<StringCollection> m_imageTypes;
        private LazyField<StringCollection> m_sections;
        private LazyField<StringCollection> m_typeIds;

        public ThingTypeGetParams()
        {
            SetSections(ThingTypeSectionType.Standard);
        }

        [XmlElement("id", Order = 0)]
        public StringCollection TypeIds
        {
            get { return m_typeIds.Value; }
            set { m_typeIds.Value = value; }
        }

        [XmlElement("section", Order = 1)]
        public StringCollection Sections
        {
            get { return m_sections.Value; }
            set { m_sections.Value = value; }
        }

        [XmlElement("image-type", Order = 2)]
        public StringCollection ImageTypes
        {
            get { return m_imageTypes.Value; }
            set { m_imageTypes.Value = value; }
        }

        [XmlElement("last-client-refresh", Order = 3)]
        public DateTime LastClientRefresh { get; set; }

        #region IHealthVaultTypeSerializable Members

        public void Validate()
        {
            TypeIds.ValidateOptional<string>("TypeIds");
            Sections.ValidateOptional<string>("Sections");
            ImageTypes.ValidateOptional<string>("ImageTypes");
            LastClientRefresh.ValidateOptional("LastClientRefresh");
        }

        public string Serialize()
        {
            return this.ToXml();
        }

        #endregion

        public void SetSections(ThingTypeSectionType section)
        {
            Sections.Clear();

            if ((section & ThingTypeSectionType.Core) != 0)
            {
                Sections.Add(ThingTypeSections.Core);
            }
            if ((section & ThingTypeSectionType.Xsd) != 0)
            {
                Sections.Add(ThingTypeSections.Xsd);
            }
            if ((section & ThingTypeSectionType.Columns) != 0)
            {
                Sections.Add(ThingTypeSections.Columns);
            }
            if ((section & ThingTypeSectionType.Transforms) != 0)
            {
                Sections.Add(ThingTypeSections.Transforms);
            }
            if ((section & ThingTypeSectionType.TransformSource) != 0)
            {
                Sections.Add(ThingTypeSections.TransformSource);
            }
            if ((section & ThingTypeSectionType.Versions) != 0)
            {
                Sections.Add(ThingTypeSections.Versions);
            }
        }
    }
}