// (c) Microsoft. All rights reserved
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using HealthVault.Foundation.Methods;
using HealthVault.Foundation.Types;

namespace HealthVault.Foundation
{
    public class RecordMethods
    {
        private readonly HealthVaultClient m_client;

        public RecordMethods(HealthVaultClient client)
        {
            if (client == null)
            {
                throw new ArgumentNullException("client");
            }

            m_client = client;
        }

        public async Task<TResult> GetThingsAsync<TResult>(
            RecordReference record, Array queries, CancellationToken cancelToken)
        {
            if (record == null)
            {
                throw new ArgumentNullException("record");
            }
            if (queries == null)
            {
                throw new ArgumentNullException("queries");
            }

            var requestBody = new RequestBody(queries);
            var method = new GetThings(m_client, record, requestBody, typeof (TResult));
            Response response = await method.ExecuteAsync(cancelToken);
            return (TResult) response.GetResult();
        }

        public async Task<TResult> PutThingsAsync<TResult>(
            RecordReference record, Array things, CancellationToken cancelToken)
        {
            return await PutThingsAsyncImpl<TResult>(record, things, cancelToken);
        }

        public async Task<TResult> PutThingsRawAsync<TResult>(
            RecordReference record, string xml, CancellationToken cancelToken)
        {
            if (String.IsNullOrEmpty(xml))
            {
                throw new ArgumentNullException("xml");
            }

            return await PutThingsAsyncImpl<TResult>(record, xml, cancelToken);
        }

        private async Task<TResult> PutThingsAsyncImpl<TResult>(
            RecordReference record, object things, CancellationToken cancelToken)
        {
            if (record == null)
            {
                throw new ArgumentNullException("record");
            }
            if (things == null)
            {
                throw new ArgumentNullException("things");
            }

            var requestBody = new RequestBody(things);

            var method = new PutThings(m_client, record, requestBody, typeof(TResult));
            Response response = await method.ExecuteAsync();
            return (TResult)response.GetResult();
        }

        public async Task RemoveThingsAsync(RecordReference record, Array keys, CancellationToken cancelToken)
        {
            if (record == null)
            {
                throw new ArgumentNullException("record");
            }
            if (keys == null)
            {
                throw new ArgumentNullException("keys");
            }

            var body = new RequestBody(keys);

            var method = new RemoveThings(m_client, record, body);
            Response response = await method.ExecuteAsync(cancelToken);

            response.EnsureSuccess();
        }

        public async Task<BlobPutInfo> BeginPutBlobAsync(RecordReference record, CancellationToken cancelToken)
        {
            if (record == null)
            {
                throw new ArgumentNullException("record");
            }

            var beginBlobMethod = new BeginPutBlob(m_client, record);
            Response response = await beginBlobMethod.ExecuteAsync(cancelToken);
            return (BlobPutInfo) response.GetResult();
        }

        public async Task<Uri> UploadBlobStreamAsync(
            RecordReference record, Stream stream, string contentType, CancellationToken cancelToken)
        {
            if (record == null)
            {
                throw new ArgumentNullException("record");
            }
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            BlobPutInfo putInfo = await BeginPutBlobAsync(record, cancelToken);
            if (stream.Length > putInfo.MaxSize)
            {
                throw new ClientException(ClientError.StreamTooLarge);
            }

            var blobUri = new Uri(putInfo.Url);
            await m_client.Streamer.UploadAsync(blobUri, stream, putInfo.ChunkSize, cancelToken);

            return blobUri;
        }

        public async Task RemoveApplicationRecordAuthorizationAsync(RecordReference record, CancellationToken cancelToken)
        {
            if (record == null)
            {
                throw new ArgumentNullException("record");
            }

            var method = new RemoveApplicationRecordAuthorization(m_client, record);
            Response response = await method.ExecuteAsync(cancelToken);

            response.EnsureSuccess();
        }

        public async Task<TResult> QueryPermissionsAsync<TResult>(
            RecordReference record, 
            object requestParams,
            CancellationToken cancelToken)
        {
            if (record == null)
            {
                throw new ArgumentNullException("record");
            }

            if (requestParams == null)
            {
                throw new ArgumentNullException("requestParams");
            }

            RequestBody requestBody = new RequestBody(requestParams);
            var queryPermissionsMethod = new QueryPermissions(m_client, record, requestBody, typeof(TResult));
            Response response = await queryPermissionsMethod.ExecuteAsync(cancelToken);
            return (TResult)response.GetResult();
        }
    }
}