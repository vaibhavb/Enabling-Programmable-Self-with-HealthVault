// (c) Microsoft. All rights reserved
using System.Xml.Serialization;

namespace HealthVault.Foundation
{
    public class ServerError : IValidatable
    {
        [XmlElement("message", Order = 1)]
        public string Message { get; set; }

        [XmlElement("context", Order = 2)]
        public ServerErrorContext Context { get; set; }

        [XmlElement("error-info", Order = 3)]
        public string ErrorInfo { get; set; }

        #region IValidatable Members

        public void Validate()
        {
            Message.ValidateRequired("Message");
        }

        #endregion

        public override string ToString()
        {
            if (string.IsNullOrEmpty(ErrorInfo))
            {
                return Message;
            }

            return string.Format("{0}, {1}", Message, ErrorInfo);
        }
    }
}