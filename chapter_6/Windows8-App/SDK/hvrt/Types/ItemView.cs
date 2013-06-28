// (c) Microsoft. All rights reserved

using System.Xml.Serialization;
using HealthVault.Foundation;

namespace HealthVault.Types
{
    public sealed class ItemView : IHealthVaultTypeSerializable
    {
        private LazyField<StringCollection> m_sections;
        private LazyField<StringCollection> m_typeVersions;
        private LazyField<StringCollection> m_xmlView;

        public ItemView()
            : this(ItemSectionType.Standard)
        {
        }

        public ItemView(ItemSectionType types)
        {
            SetSections(types);
        }

        [XmlElement("section", Order = 1)]
        public StringCollection Sections
        {
            get { return m_sections.Value; }
            set { m_sections.Value = value; }
        }

        [XmlElement("xml", Order = 2)]
        public StringCollection XmlView
        {
            get { return m_xmlView.Value; }
            set { m_xmlView.Value = value; }
        }

        [XmlElement("type-version-format", Order = 3)]
        public StringCollection TypeVersions
        {
            get { return m_typeVersions.Value; }
            set { m_typeVersions.Value = value; }
        }

        internal bool ReturnsTypedData
        {
            get { return (m_xmlView.HasValue && m_xmlView.Value.Contains(string.Empty)); }
        }

        #region IHealthVaultTypeSerializable Members

        public string Serialize()
        {
            return Serialize();
        }

        public void Validate()
        {
            Sections.ValidateRequired<string>("Sections");
        }

        #endregion

        public void SetSections(ItemSectionType types)
        {
            UpdateSections(types);
        }

        public static ItemView Deserialize(string xml)
        {
            return HealthVaultClient.Serializer.FromXml<ItemView>(xml);
        }

        private void UpdateSections(ItemSectionType section)
        {
            StringCollection sections = Sections;

            if ((section & ItemSectionType.Core) != 0)
            {
                sections.Add(ItemSections.Core);
            }
            if ((section & ItemSectionType.Audits) != 0)
            {
                sections.Add(ItemSections.Audits);
            }
            if ((section & ItemSectionType.Blobs) != 0)
            {
                sections.Add(ItemSections.Blobs);
            }
            if ((section & ItemSectionType.Tags) != 0)
            {
                sections.Add(ItemSections.Tags);
            }
            if ((section & ItemSectionType.EffectivePermissions) != 0)
            {
                sections.Add(ItemSections.Permissions);
            }
            if ((section & ItemSectionType.Signatures) != 0)
            {
                sections.Add(ItemSections.Signatures);
            }
            if ((section & ItemSectionType.Data) != 0)
            {
                UpdateXml(string.Empty);
            }
        }

        private void UpdateXml(string section)
        {
            if (!XmlView.Contains(section))
            {
                XmlView.Add(section);
            }
        }
    }
}