// (c) Microsoft. All rights reserved
using System;
using System.Text;

namespace HealthVault.Foundation
{
    public class ClientException : Exception
    {
        private readonly ClientError m_error;

        public ClientException(ClientError error)
            : this(error, null, null)
        {
        }

        public ClientException(ClientError error, string message, Exception innerEx)
            : base(CreateErrorMessage(error, message), innerEx)
        {
            m_error = error;
            HResult = (int) (HResults.ClientErrorBase | (uint) error);
        }

        public ClientError Error
        {
            get { return m_error; }
        }

        private static string CreateErrorMessage(ClientError error, string message)
        {
            var builder = new StringBuilder();
            builder.AppendLine(error.ToString());
            if (!string.IsNullOrEmpty(message))
            {
                builder.AppendLine(message);
            }
            return builder.ToString();
        }
    }
}