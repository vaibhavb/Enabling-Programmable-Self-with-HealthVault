// (c) Microsoft. All rights reserved
using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace HealthVault.Foundation
{
    [XmlRoot("response")]
    public class Response : IXmlSerializable
    {
        [XmlIgnore]
        public ResponseStatus Status { get; set; }

        [XmlIgnore]
        public object Body { get; set; }

        [XmlIgnore]
        public Request Request { get; set; }

        [XmlIgnore]
        public bool HasError
        {
            get { return (Status != null && Status.HasError); }
        }

        [XmlIgnore]
        public bool IsSuccess
        {
            get { return !HasError; }
        }

        #region IXmlSerializable Members

        public XmlSchema GetSchema()
        {
            throw new NotImplementedException();
        }

        public void ReadXml(XmlReader reader)
        {
            reader.MoveToContent();
            reader.ReadStartElement("response");
            {
                ReadStatus(reader);
                if (reader.IsStartElement())
                {
                    ReadBody(reader);
                }
            }
            reader.ReadEndElement();
        }

        public void WriteXml(XmlWriter writer)
        {
            throw new NotSupportedException();
        }

        #endregion

        public void EnsureSuccess()
        {
            if (HasError)
            {
                throw new ServerException(Status.StatusCode, Status.Error);
            }
        }

        public object GetResult()
        {
            EnsureSuccess();
            return Body;
        }

        public T GetResult<T>()
        {
            return (T) GetResult();
        }

        private void ReadStatus(XmlReader reader)
        {
            Status = HealthVaultClient.Serializer.Deserialize<ResponseStatus>(reader);
        }

        private void ReadBody(XmlReader reader)
        {
            var hvReader = reader as HealthVaultXmlReader;
            ResponseDeserializationContext context = null;
            if (hvReader != null)
            {
                context = hvReader.Context as ResponseDeserializationContext;
            }

            if (context == null || context.BodyType == null)
            {
                Body = reader.ReadOuterXml();
            }
            else
            {
                hvReader.RootElementName = context.BodyType.Name;
                Body = HealthVaultClient.Serializer.Deserialize(reader, context.BodyType);
            }
        }
    }
}