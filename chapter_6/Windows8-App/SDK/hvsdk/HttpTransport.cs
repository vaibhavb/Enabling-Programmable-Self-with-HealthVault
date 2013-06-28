// (c) Microsoft. All rights reserved
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace HealthVault.Foundation
{
    public class HttpTransport : IHttpTransport
    {
        private HttpClient m_client;
        private HttpClientHandler m_handler;
        private int m_maxAttempts;
        private string m_serviceUrl;

        public HttpTransport(string serviceUrl)
        {
            m_handler = new HttpClientHandler();
            m_client = new HttpClient(m_handler, true);

            ServiceUrl = serviceUrl;
            MaxAttempts = 2;
            Timeout = TimeSpan.FromSeconds(30);
            Compression = true;
        }

        public int MaxAttempts
        {
            get { return m_maxAttempts; }
            set
            {
                if (value <= 0)
                {
                    value = 1;
                }
                m_maxAttempts = value;
            }
        }

        public HttpClient HttpClient
        {
            get { return m_client; }
        }

        #region IHttpTransport Members

        public string ServiceUrl
        {
            get { return m_serviceUrl; }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentException("ServiceUrl");
                }
                m_serviceUrl = value;
            }
        }

        public TimeSpan Timeout
        {
            get { return m_client.Timeout; }
            set { m_client.Timeout = value; }
        }

        public bool Compression
        {
            get { return (m_handler.AutomaticDecompression != DecompressionMethods.None); }
            set { m_handler.AutomaticDecompression = (value) ? DecompressionMethods.GZip : DecompressionMethods.None; }
        }

        public async Task<HttpResponseMessage> SendAsync(HttpContent content, CancellationToken cancelToken)
        {
            if (content == null)
            {
                throw new ArgumentNullException("content");
            }

            for (int attempt = 1; attempt <= m_maxAttempts; ++attempt)
            {
                HttpResponseMessage responseMessage = null;
                try
                {
                    responseMessage = await AttemptSendAsync(content, cancelToken);
                    if (!IsServerError(responseMessage.StatusCode) || attempt == m_maxAttempts)
                    {
                        responseMessage.EnsureSuccessStatusCode(); // Throws if failure

                        HttpResponseMessage response = responseMessage;
                        responseMessage = null;

                        return response;
                    }
                }
                catch (WebException ex)
                {
                    if (!ShouldRetry(ex) || attempt == m_maxAttempts)
                    {
                        NotifyError(ex);
                        throw;
                    }
                }
                finally
                {
                    if (responseMessage != null)
                    {
                        responseMessage.Dispose();
                    }
                }
            }

            throw new WebException("Maximum attempts", WebExceptionStatus.SendFailure);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        public event EventHandler<Exception> Error;
        public event EventHandler<HttpRequestMessage> SendingRequest;
        public event EventHandler<HttpResponseMessage> ReceivedResponse;

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (m_client != null)
                {
                    m_client.Dispose();
                    m_client = null;
                    m_handler = null; // m_client disposes handler
                }
            }
        }

        private async Task<HttpResponseMessage> AttemptSendAsync(HttpContent content, CancellationToken cancelToken)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Post, ServiceUrl))
            {
                request.Content = content;
                NotifySending(request);

                HttpResponseMessage response = await m_client.SendAsync(request, cancelToken);

                NotifyReceived(response);

                return response;
            }
        }

        private void NotifySending(HttpRequestMessage message)
        {
            if (SendingRequest != null)
            {
                SendingRequest(this, message);
            }
        }

        private void NotifyReceived(HttpResponseMessage message)
        {
            if (ReceivedResponse != null)
            {
                ReceivedResponse(this, message);
            }
        }

        private void NotifyError(Exception ex)
        {
            if (Error != null)
            {
                Error(this, ex);
            }
        }

        private bool IsServerError(HttpStatusCode code)
        {
            return ((int) code >= 500);
        }

        private bool ShouldRetry(WebException ex)
        {
            switch (ex.Status)
            {
                case WebExceptionStatus.RequestCanceled:
                case WebExceptionStatus.MessageLengthLimitExceeded:
                    return false;
            }

            return (true);
        }
    }
}