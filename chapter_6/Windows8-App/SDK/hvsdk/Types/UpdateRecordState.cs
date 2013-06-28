// (c) Microsoft. All rights reserved

using System.Xml.Serialization;

namespace HealthVault.Foundation.Types
{
    [XmlType("record-state")]
    public class UpdateRecordState
    {
        public UpdateRecordState()
        {
        }

        public UpdateRecordState(string value)
        {
            this.Value = value;
        }

        [XmlText]
        public string Value
        {
            get;
            set;
        }
    }
}
