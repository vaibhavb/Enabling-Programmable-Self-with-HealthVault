// (c) Microsoft. All rights reserved
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace HealthVault.Foundation
{
    public interface IHttpTransport : IDisposable
    {
        string ServiceUrl { get; set; }
        TimeSpan Timeout { get; set; }
        bool Compression { get; set; }

        Task<HttpResponseMessage> SendAsync(HttpContent content, CancellationToken cancelToken);
    }
}