// (c) Microsoft. All rights reserved
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace HealthVault.Foundation
{
    public class HttpStreamer : IHttpStreamer
    {
        public const string OctetStreamMimeType = "application/octet-stream";

        private HttpClient m_client;

        public HttpStreamer()
        {
            m_client = new HttpClient();
            m_client.DefaultRequestHeaders.ExpectContinue = false;
        }

        #region IHttpStreamer Members

        public async Task DownloadAsync(Uri uri, Stream destination, CancellationToken cancelToken)
        {
            if (uri == null)
            {
                throw new ArgumentNullException("uri");
            }

            using (Stream stream = await m_client.GetStreamAsync(uri))
            {
                await stream.CopyToAsync(destination, 4096, cancelToken);
                await destination.FlushAsync();
            }
        }

        public async Task UploadAsync(Uri uri, Stream source, int chunkSize, CancellationToken cancelToken)
        {
            if (uri == null)
            {
                throw new ArgumentNullException("uri");
            }
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            var buffer = new byte[chunkSize];
            int countUploaded = 0;
            var length = (int) source.Length;
            var binaryContentType = new MediaTypeHeaderValue(OctetStreamMimeType);

            while (countUploaded < length)
            {
                int currentChunkLength = Math.Min(chunkSize, length - countUploaded);
                await source.ReadAsync(buffer, 0, currentChunkLength, cancelToken);

                var request = new HttpRequestMessage(HttpMethod.Post, uri);

                var content = new ByteArrayContent(buffer, 0, currentChunkLength);
                content.Headers.ContentType = binaryContentType;
                content.Headers.ContentRange = new ContentRangeHeaderValue(
                    countUploaded, (countUploaded + currentChunkLength) - 1);
                request.Content = content;

                if (currentChunkLength < chunkSize)
                {
                    request.Headers.Add("x-hv-blob-complete", "1");
                }

                HttpResponseMessage response = await m_client.SendAsync(request, cancelToken);
                response.EnsureSuccessStatusCode();

                countUploaded += currentChunkLength;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (m_client != null)
                {
                    m_client.Dispose();
                    m_client = null;
                }
            }
        }
    }
}