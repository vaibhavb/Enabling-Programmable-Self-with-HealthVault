// Copyright (c) Microsoft Corp.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//     http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace Microsoft.Health.Mobile
{
    /// <summary>
    /// The data related to a web request.
    /// </summary>
    public class SendPostEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the web request.
        /// </summary>
        public WebRequest WebRequest { get; set; }

        /// <summary>
        /// Gets or sets the request data.
        /// </summary>
        public string RequestData { get; set; }

        /// <summary>
        /// Gets or sets the HealthVaultRequest instance.
        /// </summary>
        public HealthVaultRequest HealthVaultRequest { get; set; }

        /// <summary>
        /// Gets or sets the response.
        /// </summary>
        public WebResponse WebResponse { get; set; }

        /// <summary>
        /// Gets or sets the response data.
        /// </summary>
        public string ResponseData { get; set; }

        /// <summary>
        /// Gets or sets the response handler. 
        /// </summary>
        public EventHandler<SendPostEventArgs> ResponseCallback { get; set; }

        /// <summary>
        /// Gets or sets the error text.
        /// </summary>
        public string ErrorText { get; set; }
    }

    /// <summary>
    /// Class to simplify making POSTs and getting the responses. 
    /// </summary>
    public class WebTransport
    {
        private static List<string> _requestResponseLog = new List<string>();

        /// <summary>
        /// Gets or sets whether all requests and responses should be logged
        /// </summary>
        /// <remarks>
        /// The log is global across all instances of the class, so if there are
        /// overlapped operations the requests and responses may be out of order.
        /// </remarks>
        public static bool RequestResponseLogEnabled { get; set; }

        /// <summary>
        /// Gets the request/response log.
        /// </summary>
        public static IList<string> RequestResponseLog
        {
            get { return _requestResponseLog; }
        }

        /// <summary>
        /// Begin to send a post request to a specific url. 
        /// </summary>
        /// <param name="url">The target url.</param>
        /// <param name="requestData">The data to post to the url.</param>
        /// <param name="responseCallback">The completion routine.</param>
        /// <param name="userRequest">User parameter.</param>
        public virtual void BeginSendPostRequest(
            string url,
            string requestData,
            EventHandler<SendPostEventArgs> responseCallback,
            HealthVaultRequest userRequest)
        {
            if (RequestResponseLogEnabled)
            {
                _requestResponseLog.Add(requestData);
            }
          
            SendPostEventArgs args = new SendPostEventArgs();
            args.RequestData = requestData;
            args.HealthVaultRequest = userRequest;
            args.ResponseCallback = responseCallback;

            if (userRequest.WebRequest == null)
            {
                args.WebRequest = WebRequest.Create(url);
                args.WebRequest.Method = "POST";
            }
            else
            {
                    // Mock case...
                args.WebRequest = userRequest.WebRequest;
            }

            try
            {
                IAsyncResult result = args.WebRequest.BeginGetRequestStream(GetRequestStreamCallback, args);
            }
            catch (Exception e)
            {
                InvokeResponseCallback(args, e.ToString());
                return;
            }
        }

        /// <summary>
        /// Sends the request post data.
        /// </summary>
        /// <param name="asyncResult">The async result.</param>
        private void GetRequestStreamCallback(IAsyncResult asyncResult)
        {
            SendPostEventArgs args = (SendPostEventArgs)asyncResult.AsyncState;

            using (Stream postStream = args.WebRequest.EndGetRequestStream(asyncResult))
            {
                byte[] bytes = Encoding.UTF8.GetBytes(args.RequestData);
                postStream.Write(bytes, 0, bytes.Length);
            }

            args.WebRequest.BeginGetResponse(ResponseCallback, args);
        }

        /// <summary>
        /// Processes the response and sends it off to the caller.
        /// </summary>
        /// <param name="asyncResult">The async result.</param>
        private void ResponseCallback(IAsyncResult asyncResult)
        {
            SendPostEventArgs args = (SendPostEventArgs)asyncResult.AsyncState;

            try
            {
                args.WebResponse = args.WebRequest.EndGetResponse(asyncResult);
            }
            catch (Exception e) 
            {
                InvokeResponseCallback(args, e.ToString());
                return;
            }

            Stream responseStream = args.WebResponse.GetResponseStream();
            using (StreamReader reader = new StreamReader(responseStream))
            {
                args.ResponseData = reader.ReadToEnd();
            }

            if (RequestResponseLogEnabled)
            {
                _requestResponseLog.Add(args.ResponseData);
            }

            InvokeResponseCallback(args, null);
        }

        /// <summary>
        /// Call the user's callback handler.
        /// </summary>
        /// <param name="args">The arguments to pass.</param>
        /// <param name="errorText">The error text.</param>
        private void InvokeResponseCallback(
            SendPostEventArgs args,
            string errorText)
        {
            args.ErrorText = errorText;

            if (args.ResponseCallback != null)
            {
                args.ResponseCallback(this, args);
            }
        }
    }
}
