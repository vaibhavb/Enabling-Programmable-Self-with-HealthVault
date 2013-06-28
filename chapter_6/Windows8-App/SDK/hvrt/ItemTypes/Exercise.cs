// (c) Microsoft. All rights reserved

using System;
using System.Xml.Serialization;
using HealthVault.Foundation;
using HealthVault.Types;

namespace HealthVault.ItemTypes
{
    [XmlType(RootElement)]
    public sealed class Exercise : IItemDataTyped
    {
        internal const string TypeIDString = "85a21ddb-db20-4c65-8d30-33c899ccf612";
        internal const string RootElement = "exercise";
        private StructuredNameValueCollection m_details;
        private ItemProxy m_itemProxy;
        private ExerciseSegmentCollection m_segments;

        public Exercise()
        {
            Title = String.Empty;
            m_itemProxy = new ItemProxy(TypeID, this);
        }

        public static string TypeID
        {
            get { return TypeIDString; }
        }

        [XmlElement("when", Order = 1)]
        public ApproxDateTime When { get; set; }

        [XmlElement("activity", Order = 2)]
        public CodableValue Activity { get; set; }

        [XmlElement("title", Order = 3)]
        public string Title { get; set; }

        [XmlElement("distance", Order = 4)]
        public LengthMeasurement Distance { get; set; }

        [XmlElement("duration", Order = 5)]
        public PositiveDouble Duration { get; set; }

        [XmlElement("detail", Order = 6)]
        public StructuredNameValueCollection Details
        {
            get
            {
                if (m_details == null)
                {
                    m_details = new StructuredNameValueCollection();
                }

                return m_details;
            }
            set { m_details = value; }
        }

        [XmlElement("segment", Order = 7)]
        public ExerciseSegmentCollection Segments
        {
            get
            {
                if (m_segments == null)
                {
                    m_segments = new ExerciseSegmentCollection();
                }

                return m_segments;
            }
            set { m_segments = value; }
        }

        #region IItemDataTyped

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

        #endregion

        #region IItemDataTyped Members

        public void Validate()
        {
            When.ValidateRequired("When");
            Activity.ValidateRequired("Activity");
            Distance.ValidateOptional("Distance");
            Duration.ValidateOptional("Duration");
            Details.ValidateOptional<StructuredNameValue>("Details");
            Segments.ValidateOptional<ExerciseSegment>("Segments");
        }

        public string Serialize()
        {
            return this.ToXml();
        }

        public HealthVault.Types.DateTime GetDateForEffectiveDate()
        {
            return (this.When != null) ? this.When.ToDateTime() : null;
        }

        #endregion

        public static Exercise Deserialize(string xml)
        {
            return HealthVaultClient.Serializer.FromXml<Exercise>(xml);
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
            return String.Format("{0}", Activity.Text);
        }

        public bool ShouldSerializeTitle()
        {
            return !String.IsNullOrEmpty(Title);
        }

        //-----------------------------------------
        //
        // Vocabularies
        //
        //-----------------------------------------
        public static VocabIdentifier VocabForExerciseActivities()
        {
            return new VocabIdentifier(VocabFamily.HealthVault, VocabName.ExerciseActivities);
        }

        public static VocabIdentifier VocabForExerciseUnits()
        {
            return new VocabIdentifier(VocabFamily.HealthVault, VocabName.ExerciseUnits);
        }

        public static VocabIdentifier VocabForExerciseDetailNames()
        {
            return new VocabIdentifier(VocabFamily.HealthVault, VocabName.ExerciseDetailNames);
        }
    }
}