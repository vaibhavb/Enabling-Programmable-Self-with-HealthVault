// (c) Microsoft. All rights reserved
using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using HealthVault.Foundation.Types;

namespace HealthVault.Foundation
{
    [XmlRoot("info")]
    public class RequestBody : IXmlSerializable, IValidatable
    {
        private object m_data;
        private string m_xml;

        public RequestBody()
        {
        }

        public RequestBody(object data)
        {
            Data = data;
        }

        public virtual object Data
        {
            get { return m_data; }
            set
            {
                m_xml = null;
                m_data = value;
            }
        }

        public Func<object, string> DataSerializer { get; set; }

        protected string RenderedXml
        {
            get { return m_xml; }
            set { m_xml = value; }
        }

        #region IValidatable Members

        public virtual void Validate()
        {
            var data = m_data as IValidatable;
            if (data != null)
            {
                data.Validate();
            }
        }

        #endregion

        #region IXmlSerializable Members

        public XmlSchema GetSchema()
        {
            return null;
        }

        public virtual void ReadXml(XmlReader reader)
        {
            Data = reader.ReadOuterXml();
        }

        public virtual void WriteXml(XmlWriter writer)
        {
            writer.WriteRaw(GetXml());
        }

        #endregion

        public string GetXml()
        {
            if (m_xml != null)
            {
                return m_xml;
            }

            if (Data == null)
            {
                m_xml = string.Empty;
                return m_xml;
            }

            m_xml = Data as string;
            if (m_xml == null)
            {
                m_xml = DataToXml();
            }

            return m_xml;
        }

        protected virtual string DataToXml()
        {
            if (DataSerializer != null)
            {
                return DataSerializer(Data);
            }

            var objArray = Data as Array;
            if (objArray != null)
            {
                return objArray.ToXml(false);
            }

            return Data.ToXml();
        }

        public Hash Hash(ICryptographer cryptographer)
        {
            return cryptographer.Hash(this.ToXml());
        }
    }
}