using HealthVault.Foundation.Methods;
// (c) Microsoft. All rights reserved
using System.Threading;
using System.Xml.Serialization;

namespace HealthVault.Foundation
{
    [XmlRoot("request", Namespace = Namespace)]
    public class Request : IValidatable
    {
        public const string Namespace = "urn:com.microsoft.wc.request";

        private static long s_nextId;

        private readonly long m_requestId;
        private RequestBody m_body;
        private RequestHeader m_header;
        private bool m_shouldEnsureOnlineToken = true;

        public Request()
            : this(null, 0)
        {
        }

        public Request(string method, int methodVersion)
            : this(method, methodVersion, null)
        {
        }

        public Request(string method, int methodVersion, object bodyData)
            : this(method, methodVersion, new RequestBody(bodyData))
        {
        }

        public Request(string method, int methodVersion, RequestBody body)
        {
            m_requestId = NextId();

            if (method != null)
            {
                Header.Method = method;
            }
            if (methodVersion > 0)
            {
                Header.MethodVersion = methodVersion;
            }
            Body = body;
        }

        [XmlElement("auth", Order = 1)]
        public RequestAuth Auth { get; set; }

        [XmlElement("header", Order = 2)]
        public RequestHeader Header
        {
            get
            {
                if (m_header == null)
                {
                    m_header = RequestHeader.CreateDefault();
                }

                return m_header;
            }
            set { m_header = value; }
        }

        [XmlElement("info", Order = 3)]
        public RequestBody Body
        {
            get
            {
                if (m_body == null)
                {
                    m_body = new RequestBody();
                }

                return m_body;
            }
            set { m_body = value; }
        }

        /// <summary>
        /// Request targets this record
        /// </summary>
        [XmlIgnore]
        public RecordReference Record { get; set; }

        /// <summary>
        /// Anonymous requests don't require authentication headers/credentials
        /// </summary>
        [XmlIgnore]
        public bool IsAnonymous { get; set; }

        [XmlIgnore]
        internal bool ShouldEnsureOnlineToken
        {
            get { return m_shouldEnsureOnlineToken; }
            set { m_shouldEnsureOnlineToken = value; }
        }

        [XmlIgnore]
        public bool IsRecordMethod
        {
            get { return (Record != null); }
        }

        // For logging, correlation etc..
        [XmlIgnore]
        public long Id
        {
            get { return m_requestId; }
        }

        #region IValidatable Members

        public void Validate()
        {
            Auth.ValidateOptional();
            Header.ValidateRequired("Header");
            Body.ValidateRequired("Body");
        }

        #endregion

        private static long NextId()
        {
            return Interlocked.Increment(ref s_nextId);
        }
    }
}