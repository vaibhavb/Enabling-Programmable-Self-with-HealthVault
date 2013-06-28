// (c) Microsoft. All rights reserved
using System;
using System.Text;

namespace HealthVault.Foundation
{
    public class ServerException : Exception
    {
        private readonly ServerError m_error;
        private readonly int m_statusCode;

        public ServerException(ServerStatusCode code)
            : this((int) code, null)
        {
        }

        public ServerException(int statusCode, ServerError error)
            : this(statusCode, error, null, null)
        {
        }

        public ServerException(int statusCode, ServerError error, string message, Exception innerEx)
            : base(CreateErrorMessage(statusCode, error, message), innerEx)
        {
            m_statusCode = statusCode;
            m_error = error;
            HResult = (int) (HResults.ServerErrorBase | (uint) statusCode);
        }

        public int StatusCode
        {
            get { return m_statusCode; }
        }

        public ServerError Error
        {
            get { return m_error; }
        }

        public bool IsStatusCode(ServerStatusCode code)
        {
            return (m_statusCode == (int) code);
        }

        public bool IsAccessDenied
        {
            get
            {
                switch(m_statusCode)
                {
                    default:
                        return false;

                    case (int) ServerStatusCode.AccessDenied:
                    case (int) ServerStatusCode.InvalidApp:
                    case (int) ServerStatusCode.InvalidApplicationAuthorization:
                        break;
                }

                return true;
            }
        }

        public bool IsInvalidTarget
        {
            get
            {
                switch (m_statusCode)
                {
                    default:
                        return false;

                    case (int)ServerStatusCode.InvalidRecord:
                    case (int)ServerStatusCode.InvalidPerson:
                        break;
                }

                return true;
            }
        }

        public bool IsItemNotFound
        {
            get { return this.IsStatusCode(ServerStatusCode.InvalidItem);}
        }

        public bool IsVersionStampMismatch
        {
            get { return this.IsStatusCode(ServerStatusCode.VersionStampMismatch);}
        }

        public bool IsServerError
        {
            get 
            { 
                return  (this.IsStatusCode(ServerStatusCode.Failed) || 
                         this.IsStatusCode(ServerStatusCode.RequestTimedOut));
            }
        }

        private static string CreateErrorMessage(int statusCode, ServerError error, string message)
        {
            var builder = new StringBuilder();
            builder.AppendFormat("StatusCode = {0}", statusCode);
            builder.AppendLine();
            if (error != null)
            {
                builder.AppendLine(error.ToString());
            }
            if (!string.IsNullOrEmpty(message))
            {
                builder.AppendLine(message);
            }
            return builder.ToString();
        }
    }
}