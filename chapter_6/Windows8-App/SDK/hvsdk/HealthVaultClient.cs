// (c) Microsoft. All rights reserved
using System;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HealthVault.Foundation.Methods;
using HealthVault.Foundation.Types;
using Windows.Foundation.Collections;
using Windows.Security.Authentication.OnlineId;
using Windows.Security.Authentication.Web;
using Windows.Storage;

namespace HealthVault.Foundation
{
    public class HealthVaultClient : IDisposable
    {
        private const int DefaultBufferSize = 16*1024; // bytes
        public const string MBISSLAuthPolicy = "MBI_SSL";
        public const string HBIAuthPolicy = "HBI";

        private static ISerializer s_serializer;
        private static string s_version;

        private readonly AppInfo m_appInfo;
        private readonly IWebAuthorizer m_authorizer;
        private readonly ICryptographer m_cryptographer;
        private readonly RecordMethods m_recordMethods;
        private readonly ISecretStore m_secretStore;
        private ServiceInfo m_serviceInfo;
        private readonly ServiceMethods m_serviceMethods;
        private readonly Shell m_shell;
        private readonly IHttpStreamer m_streamer;

        private UserIdentity m_userIdentity;
        private ClientState m_state;
        private IHttpTransport m_transport;
        private GetServiceDefinitionResponse m_topologyServiceDefinition;

        static HealthVaultClient()
        {
            s_serializer = new Serializer();
            s_version = CreateVersionString();
        }

        private static string CreateVersionString()
        {
            string fileVersion = typeof(HealthVaultClient).GetTypeInfo().Assembly.GetName().Version.ToString();

            return String.Format(
                CultureInfo.InvariantCulture,
                "HV-WinRT/{0}",
                fileVersion);
        }

        public HealthVaultClient(AppInfo appInfo, ServiceInfo serviceInfo, IWebAuthorizer webAuthBroker)
            : this(
                appInfo,
                serviceInfo,
                new HttpTransport(serviceInfo.ServiceUrl),
                new HttpStreamer(),
                new Cryptographer(),
                false,
                webAuthBroker)
        {
        }

        public HealthVaultClient(AppInfo appInfo, ServiceInfo serviceInfo, bool useOnlineAuthModel, IWebAuthorizer webAuthBroker)
            : this(
                appInfo,
                serviceInfo,
                new HttpTransport(serviceInfo.ServiceUrl),
                new HttpStreamer(),
                new Cryptographer(),
                useOnlineAuthModel,
                webAuthBroker)
        {
        }

        public HealthVaultClient(
            AppInfo appInfo,
            ServiceInfo serviceInfo,
            IHttpTransport transport,
            IHttpStreamer streamer,
            ICryptographer cryptographer,
            bool useOnlineAuthModel,
            IWebAuthorizer authorizer)
        {
            appInfo.ValidateRequired("appInfo");
            serviceInfo.ValidateRequired("serviceInfo");
            if (transport == null)
            {
                throw new ArgumentNullException("transport");
            }
            if (streamer == null)
            {
                throw new ArgumentNullException("streamer");
            }
            if (cryptographer == null)
            {
                throw new ArgumentNullException("cryptographer");
            }
            if (!useOnlineAuthModel && authorizer == null)
            {
                throw new ArgumentNullException("authorizer");
            }

            UseOnlineAuthModel = useOnlineAuthModel;
            m_appInfo = appInfo;
            m_serviceInfo = serviceInfo;
            m_transport = transport;
            m_streamer = streamer;
            m_cryptographer = cryptographer;
            m_authorizer = authorizer;

            m_serviceMethods = new ServiceMethods(this);
            m_recordMethods = new RecordMethods(this);
            m_shell = new Shell(this);

            m_secretStore = new SecretStore(MakeStoreName(m_appInfo.MasterAppId));
            m_state = new ClientState();
            LoadState();
        }

        internal static string Version
        {
            get { return s_version; }
        }

        public static ISerializer Serializer
        {
            get { return s_serializer; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("Serializer");
                }
                s_serializer = value;
            }
        }

        public AppInfo AppInfo
        {
            get { return m_appInfo; }
        }

        public bool UseOnlineAuthModel { get; set; }

        public ServiceInfo ServiceInfo
        {
            get { return m_serviceInfo; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("ServiceInfo");
                }

                m_serviceInfo = value;
                m_transport.ServiceUrl = m_serviceInfo.ServiceUrl;
            }
        }

        public ServiceDefinition ServiceDefinition { get; set; }

        public IHttpTransport Transport
        {
            get { return m_transport; }
        }

        public IHttpStreamer Streamer
        {
            get { return m_streamer; }
        }

        public ICryptographer Cryptographer
        {
            get { return m_cryptographer; }
        }

        public IWebAuthorizer WebAuthorizer
        {
            get { return m_authorizer; }
        }

        public ServiceMethods ServiceMethods
        {
            get { return m_serviceMethods; }
        }

        public RecordMethods RecordMethods
        {
            get { return m_recordMethods; }
        }

        public Shell Shell
        {
            get { return m_shell; }
        }

        public ClientState State
        {
            get { return m_state; }
        }

        public UserIdentity UserIdentity
        {
            get { return m_userIdentity; }
        }

        public bool IsProvisioned
        {
            get { return m_state.IsAppProvisioned; }
        }

        public string LiveIdHostName { get; set; }

        public bool Debug { get; set; }

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        //----------------------------------------
        //
        // EVENTS
        //
        //----------------------------------------
        public event EventHandler<Request> SendingRequest;
        public event EventHandler<Response> ReceivedResponse;
        //
        // Debug support
        // Invoked only if this.Debug is true
        //
        public event Action<object, Request, string> SendingXml;
        public event Action<object, Request, string> ReceivedXml;

        //----------------------------------------
        //
        // Methods
        //
        //----------------------------------------

        public async Task<Response> ExecuteRequestAsync(Request request, Type responseBodyType)
        {
            return await ExecuteRequestAsync(request, responseBodyType, CancellationToken.None);
        }

        public async Task<Response> ExecuteRequestAsync(
            Request request, Type responseBodyType,
            CancellationToken cancelToken)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            Response response = null;
            int attempt = 1;
            const int maxAttempts = 3;

            while (attempt <= maxAttempts)
            {
                //
                // Ensure we've got a session set up
                //
                SessionCredential credentials = null;
                if (!request.IsAnonymous)
                {
                    credentials = await EnsureCredentialsAsync(cancelToken);

                    if (UseOnlineAuthModel && request.ShouldEnsureOnlineToken)
                    {
                        await EnsureOnlineTokenAsync(cancelToken);
                    }
                }
                //
                // Prepare request - adding headers, session & auth information
                //
                PrepareRequestAsync(request, credentials);
                //
                // Notify any subscribers
                //
                NotifySending(request);
                //
                // Call HealthVault
                //
                response = await GetResponseAsync(request, responseBodyType, cancelToken);
                if (
                    response.IsSuccess ||
                        !(response.Status.IsStatusCredentialsExpired || 
                          response.Status.IsStatusInvalidOnlineToken ||
                          response.Status.IsStatusServerFailure) ||
                        attempt == maxAttempts)
                {
                    break;
                }

                if (response.Status.IsStatusCredentialsExpired)
                {
                    await RefreshSessionTokenAsync(cancelToken);
                }

                if (response.Status.IsStatusInvalidOnlineToken
                    && UseOnlineAuthModel 
                    && request.ShouldEnsureOnlineToken)
                {
                    await RefreshOnlineTokenAsync(cancelToken);
                }

                ++attempt;
            }

            return response;
        }

        public async Task<TResult> IsValidHealthVaultAccount<TResult>(CancellationToken cancelToken)
        {
            if (!UseOnlineAuthModel)
            {
                throw new NotSupportedException();
            }
            
            await EnsureUserIdentityAsync(cancelToken, MBISSLAuthPolicy);
            
            return await m_serviceMethods.IsValidHealthVaultAccount<TResult>(
                m_userIdentity.Tickets[0], 
                cancelToken);
        }

        public async Task<bool> CreateAccountAsync(object createAccountPersonInfo, CancellationToken cancelToken)
        {
            CreateAccountWithTicketResponse result = null;

            if (!UseOnlineAuthModel)
            {
                throw new NotSupportedException();
            }

            if (createAccountPersonInfo == null)
            {
                throw new ArgumentNullException("createAccountPersonInfo");
            }
            
            await EnsureUserIdentityAsync(cancelToken, HBIAuthPolicy);
            
            result = await m_serviceMethods.CreateAccountAsync(
                    m_userIdentity.Tickets[0],
                    createAccountPersonInfo,
                    cancelToken);

            return result != null;
        }

        public async Task<TResult> CreateRecordAsync<TResult>(object createRecordParams, CancellationToken cancelToken)
        {
            if (!UseOnlineAuthModel)
            {
                throw new NotSupportedException();
            }

            if (createRecordParams == null)
            {
                throw new ArgumentNullException("createRecordParams");
            }

            TResult result = await m_serviceMethods.CreateRecordAsync<TResult>(createRecordParams, cancelToken);
            return result;
        }

        public async Task UpdateProvisioningInfoAsync(CancellationToken cancelToken)
        {
            AppProvisioningInfo provInfo = null;

            if (UseOnlineAuthModel)
            {
                await EnsureUserIdentityAsync(cancelToken, HBIAuthPolicy);
                provInfo = await m_serviceMethods.CreateApplicationAsync(m_userIdentity.Tickets[0], m_appInfo, cancelToken);
            }
            else
            {
                provInfo = await m_serviceMethods.GetAppProvisioningInfoAsync(cancelToken);
            }

            lock (m_state)
            {
                m_state.ProvisioningInfo = provInfo;
                SaveState();
            }
        }

        private async Task HandleProvisionAppSuccess(CancellationToken cancelToken, AuthResult authResult)
        {
            string instanceId = null;

            if (!String.IsNullOrEmpty(authResult.ResponseUri))
            {
                instanceId = m_shell.ParseInstanceIdFromUri(authResult.ResponseUri);
            }

            if (instanceId == null)
            {
                throw new ClientException(ClientError.InvalidOrMissingInstanceId);
            }

            ServiceInfo serviceInfo = await GetServiceInfoForInstance(instanceId, cancelToken);

            if (serviceInfo == null)
            {
                throw new ClientException(ClientError.InvalidOrMissingInstanceId);
            }

            ServiceInfo = serviceInfo;

            await RefreshSessionTokenAsync(cancelToken); 
        }

        private async Task<ServiceInfo> GetServiceInfoForInstance(string instanceId, CancellationToken cancelToken)
        {
            if (m_topologyServiceDefinition == null)
            {
                await LoadTopologyServiceDefinition(cancelToken);
            }

            ServiceInfo serviceInfo = LoadServiceInfoForInstance(instanceId);
            if (serviceInfo == null)
            {
                // Reload the service definition and try again.
                await LoadTopologyServiceDefinition(cancelToken);
                serviceInfo = LoadServiceInfoForInstance(instanceId);
            }

            return serviceInfo;
        }

        private async Task LoadTopologyServiceDefinition(CancellationToken cancelToken)
        {
            m_topologyServiceDefinition = await m_serviceMethods.GetServiceDefinition(
                new ServiceDefinitionResponseSections[] { ServiceDefinitionResponseSections.Topology },
                cancelToken);
        }

        private ServiceInfo LoadServiceInfoForInstance(string instanceId)
        {
            foreach (Instance instance in m_topologyServiceDefinition.Instances.AllInstances)
            {
                if (instance.Id.ToLowerInvariant() == instanceId.ToLowerInvariant())
                {
                    return new ServiceInfo()
                    {
                        ServiceUrl = instance.PlatformUrl,
                        ShellUrl = instance.ShellUrl
                    };
                }
            }

            return null;
        }

        public async Task RefreshSessionTokenAsync(CancellationToken cancelToken)
        {
            SessionCredential credential = await m_serviceMethods.GetSessionTokenAsync(cancelToken);

            lock (m_state)
            {
                m_state.Credentials = credential;
                SaveState();
            }
        }

        public async Task RefreshOnlineTokenAsync(CancellationToken cancelToken)
        {
            if (!UseOnlineAuthModel)
            {
                throw new NotSupportedException();
            }

            await EnsureUserIdentityAsync(cancelToken, MBISSLAuthPolicy);
            CreateCredentialTokenWithTicketResponse onlineToken = await m_serviceMethods.CreateCredentialTokenAsync(m_userIdentity.Tickets[0], cancelToken);
            lock (m_state)
            {
                m_state.OnlineToken = onlineToken.Token;
                SaveState();
            }
        }

        public async Task<bool> IsAppAuthorizedOnServerAsync(CancellationToken cancelToken)
        {
            try
            {
                await RefreshSessionTokenAsync(CancellationToken.None);
                return m_state.HasCredentials;
            }
            catch (ServerException se)
            {
                if (!(se.IsStatusCode(ServerStatusCode.InvalidApp) ||
                    se.IsStatusCode(ServerStatusCode.AccessDenied)
                    )
                    )
                {
                    throw;
                }
            }

            return false;
        }

        public async Task<bool> EnsureUserIdentityAsync(
            CancellationToken cancelToken,
            string msaAuthPolicy)
        {
            OnlineIdServiceTicketRequest[] tickets = new OnlineIdServiceTicketRequest[]
            {
                new OnlineIdServiceTicketRequest(
                    LiveIdHostName,
                    msaAuthPolicy)
            };

            try
            {
                var onlineIdAuthenticator = new OnlineIdAuthenticator();
                m_userIdentity = await onlineIdAuthenticator.AuthenticateUserAsync(tickets, CredentialPromptType.PromptIfNeeded);
                return true;
            }
            catch(TaskCanceledException) {
                throw;
            }
            catch (Exception)
            {
                return false; ;
            }
        }

        public async Task<WebAuthenticationStatus> EnsureAppProvisionedAsync(CancellationToken cancelToken)
        {
            if (m_state.IsAppProvisioned && m_state.IsProvisionedInfoCurrent)
            {
                return WebAuthenticationStatus.Success;
            }

            //
            // Make sure we've got information to provision this app instance
            //
            if (!m_state.HasProvisioningInfo)
            {
                await UpdateProvisioningInfoAsync(cancelToken);
                m_state.IsProvisionedInfoCurrent = true;
            }

            // In case of online apps - UpdateProvisioningInfoAsync creates the app 
            // so it is ensured that the app will exist on the server
            if (UseOnlineAuthModel && m_state.IsProvisionedInfoCurrent)
            {
                return WebAuthenticationStatus.Success;
            }

            //
            // Attempt to create a session. If success, then the app was authorized using Shell
            // Else, we'll need to send the user to Shell
            //
            bool existsOnServer = await IsAppAuthorizedOnServerAsync(cancelToken);
            if (!existsOnServer)
            {
                if (!m_state.IsProvisionedInfoCurrent)
                {
                    await UpdateProvisioningInfoAsync(cancelToken);
                    m_state.IsProvisionedInfoCurrent = true;
                }

                // In case of online model (FPA), the application doesn't need shell for auth
                // purposes
                if (!UseOnlineAuthModel)
                {
                    AuthResult authResult = await m_shell.ProvisionApplicationAsync();
                    if (authResult.Status == WebAuthenticationStatus.ErrorHttp)
                    {
                        return authResult.Status;
                    }
                    else if (authResult.Status == WebAuthenticationStatus.Success)
                    {
                        await HandleProvisionAppSuccess(cancelToken, authResult);
                    }
                }
            }

            m_state.IsProvisionedInfoCurrent = true;
            return m_state.HasCredentials ? WebAuthenticationStatus.Success : WebAuthenticationStatus.UserCancel;
        }

        public async Task<PersonInfo> BatchCallsForOnlinePersonInfoAsync(CancellationToken cancelToken)
        {
            if (!UseOnlineAuthModel)
            {
                throw new NotSupportedException();
            }

            bool createApp = true;
            CreateAuthTokensWithTicketResponse response = null;

            // call createauthtokenwithticket and if the result comes back with 
            // app doesn't exist, create the app and then call again to create CAST, user_token,
            // and person info.
            try
            {
                if (m_state.HasProvisioningInfo)
                {
                    response = await m_serviceMethods
                         .CreateAuthTokensWithTicketAsync(m_userIdentity.Tickets[0], cancelToken);
                    createApp = false;
                }
            }
            catch (ServerException se)
            {
                if (se.IsStatusCode(ServerStatusCode.InvalidPerson))
                {
                    if (m_state.HasProvisioningInfo)
                    {
                        m_state.ProvisioningInfo.Clear();
                    }
                }
                else if (!(se.IsStatusCode(ServerStatusCode.InvalidApp) ||
                    se.IsStatusCode(ServerStatusCode.AccessDenied)))
                {
                    throw;
                }
            }

            if (createApp)
            {
                await EnsureAppProvisionedAsync(cancelToken);
                // App creation requires HBI auth policy so change the authPolicy to MBI_SSL
                await EnsureUserIdentityAsync(cancelToken, MBISSLAuthPolicy);
                response = await m_serviceMethods
                    .CreateAuthTokensWithTicketAsync(m_userIdentity.Tickets[0], cancelToken);
            }

            lock (m_state)
            {
                m_state.Credentials = response.SessionCredential;
                m_state.OnlineToken = response.UserAuthToken;
                SaveState();
            }

            return response.PersonInfo;
        }

        public async Task<SessionCredential> EnsureCredentialsAsync(CancellationToken cancelToken)
        {
            if (!m_state.HasCredentials)
            {
                await RefreshSessionTokenAsync(cancelToken);
            }

            return m_state.Credentials;
        }

        public async Task<string> EnsureOnlineTokenAsync(CancellationToken cancelToken)
        {
            if (!m_state.HasOnlineToken)
            {
                await RefreshOnlineTokenAsync(cancelToken);
            }

            return m_state.OnlineToken;
        }

        //----------------------------------------
        //
        // State mgmt
        //
        //----------------------------------------
        public void LoadState()
        {
            lock (m_state)
            {
                try
                {
                    ClientState state = ClientState.Load(m_secretStore);
                    m_state = state;
                }
                catch
                {
                }
            }
        }

        public void SaveState()
        {
            lock (m_state)
            {
                m_state.Save(m_secretStore);
            }
        }

        public void ResetState()
        {
            lock (m_state)
            {
                m_state.Reset(m_secretStore);
            }
        }

        public IPropertySet GetPropertyStore()
        {
            return ApplicationData.Current.GetLocalPropertySet(MakeStoreName(m_appInfo.MasterAppId));
        }

        //----------------------------------------
        //
        // Implementation
        //
        //----------------------------------------

        private async Task<Response> GetResponseAsync(
            Request request, Type responseBodyType,
            CancellationToken cancelToken)
        {
            //
            // Serialize the request
            //
            StringContent content = SerializeRequest(request);
            //
            // Call the server. 
            //
            HttpResponseMessage httpResponse = await m_transport.SendAsync(content, cancelToken);
            using (httpResponse)
            {
                //
                // Deserialize the response
                //
                if (Debug)
                {
                    return await DeserializeResponseDebug(request, httpResponse.Content, responseBodyType);
                }

                return await DeserializeResponse(request, httpResponse.Content, responseBodyType);
            }
        }

        private void PrepareRequestAsync(Request request, SessionCredential credentials)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            PrepareHeader(request, credentials);
            PrepareAuth(request, credentials);

            request.Validate();
        }

        private void PrepareHeader(Request request, SessionCredential credentials)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            PrepareStandardHeaders(request);
            PrepareBodyHash(request);
            PrepareAuthSessionHeader(request, credentials);
        }

        private void PrepareAuth(Request request, SessionCredential credentials)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }
            if (request.IsAnonymous)
            {
                return;
            }
            if (credentials == null)
            {
                throw new ArgumentNullException("credentials");
            }

            VerifyCredentials();

            string headerXml = request.Header.ToXml();
            Hmac hmac = m_cryptographer.Hmac(credentials.SharedSecret, headerXml);
            request.Auth = new RequestAuth(hmac);
        }

        private void PrepareStandardHeaders(Request request)
        {
            RequestHeader header = request.Header;
            if (request.Record != null)
            {
                header.RecordId = request.Record.RecordId;
            }
            if (!header.HasCultureCode)
            {
                header.CultureCode = m_appInfo.CultureCode;
            }
        }

        private void PrepareBodyHash(Request request)
        {
            if (!request.Header.HasBodyHash)
            {
                request.Header.BodyHash = new HashData(request.Body.Hash(m_cryptographer));
            }
        }

        private void PrepareAuthSessionHeader(Request request, SessionCredential credentials)
        {
            if (credentials == null)
            {
                return;
            }

            var session = new AuthSession();
            session.Token = credentials.Token;

            if (UseOnlineAuthModel)
            {
                if (request.ShouldEnsureOnlineToken && m_state.HasOnlineToken)
                {
                    session.OnlineToken = m_state.OnlineToken;
                }
            }
            else if (request.Record != null)
            {
                session.Person = new OfflinePersonInfo(request.Record.PersonId);
            }

            request.Header.Session = session;
        }

        private StringContent SerializeRequest(Request request)
        {
            string xml;
            using (var writer = new StringWriter())
            {
                Serializer.Serialize(writer, request, null);
                xml = writer.ToString();
            }

            NotifySending(request, xml);

            return new StringContent(xml);
        }

        private async Task<Response> DeserializeResponse(Request request, HttpContent content, Type bodyType)
        {
            using (Stream contentStream = await content.ReadAsStreamAsync())
            {
                using (var reader = new StreamReader(contentStream, Encoding.UTF8, false, DefaultBufferSize, true))
                {
                    return DeserializeResponseXml(request, reader, bodyType);
                }
            }
        }

        private async Task<Response> DeserializeResponseDebug(Request request, HttpContent content, Type bodyType)
        {
            string xml = await content.ReadAsStringAsync();
            NotifyReceived(request, xml);

            using (var reader = new StringReader(xml))
            {
                return DeserializeResponseXml(request, reader, bodyType);
            }
        }

        private Response DeserializeResponseXml(Request request, TextReader reader, Type bodyType)
        {
            var context = new ResponseDeserializationContext {BodyType = bodyType};

            var response = (Response) Serializer.Deserialize(reader, typeof (Response), context);
            response.Request = request;

            NotifyReceived(response);

            return response;
        }

        private string MakeStoreName(Guid masterAppId)
        {
            return string.Format("HealthVaultApp_{0}", masterAppId.ToString("D"));
        }

        internal void VerifyProvisioned()
        {
            if (!m_state.IsAppProvisioned)
            {
                throw new ClientException(ClientError.AppNotProvisioned);
            }
        }

        internal void VerifyHasProvisioningInfo()
        {
            if (!m_state.HasProvisioningInfo)
            {
                throw new ClientException(ClientError.NoProvisioningInfo);
            }
        }

        internal void VerifyCredentials()
        {
            if (!m_state.HasCredentials)
            {
                throw new ClientException(ClientError.NoCredentials);
            }
        }

        private void NotifySending(Request request)
        {
            if (SendingRequest != null)
            {
                SendingRequest(this, request);
            }
        }

        private void NotifySending(Request request, string xml)
        {
            if (SendingXml != null)
            {
                SendingXml(this, request, xml);
            }
        }

        private void NotifyReceived(Response response)
        {
            if (ReceivedResponse != null)
            {
                ReceivedResponse(this, response);
            }
        }

        private void NotifyReceived(Request request, string xml)
        {
            if (ReceivedXml != null)
            {
                ReceivedXml(this, request, xml);
            }
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (m_transport != null)
                {
                    m_transport.Dispose();
                    m_transport = null;
                }
            }
        }

        
    }
}