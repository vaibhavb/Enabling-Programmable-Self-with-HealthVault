// (c) Microsoft. All rights reserved
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace HealthVault.Foundation
{
    public interface IHttpStreamer : IDisposable
    {
        Task DownloadAsync(Uri uri, Stream destination, CancellationToken cancelToken);
        Task UploadAsync(Uri uri, Stream source, int chunkSize, CancellationToken cancelToken);
    }
}