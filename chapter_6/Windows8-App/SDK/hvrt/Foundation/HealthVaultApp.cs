// (c) Microsoft. All rights reserved

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using HealthVault.Foundation.Types;
using HealthVault.Store;
using Windows.Foundation;
using Windows.Security.Authentication.Web;
using Windows.Storage;
using Windows.Storage.Search;
using Windows.Storage.Streams;
using Windows.Networking.Connectivity;
using Windows.UI.Core;
using Windows.Security.Authentication.OnlineId;
using HealthVault.Types;

namespace HealthVault.Foundation
{
    public enum AppStartupStatus
    {
        Cancelled = 0,
        Success = 1,
        Pending = 2,
        Failed = 3,
        CredentialNotFound = 4
    }


    public sealed class HealthVaultApp
    {
        internal const string UserInfoKey = "UserInfo_V1";

        private readonly AppInfoProxy m_appInfo;
        private readonly HealthVaultClient m_client;

        private IServiceDefinition m_serviceDefinition;
        private readonly object m_lock;
        private readonly HealthVaultAppSettings m_appSettings; 
        private ServiceInfoProxy m_serviceInfo;
        private readonly Vocabs m_vocabs;
        private LocalVault m_localVault;
        private AppStartupStatus m_startupStatus;
        private UserInfo m_user;
        private ServerErrorNumber m_serverErrorNumber;

        private string m_liveIdHostName;
        
        public static CoreDispatcher UIDispatcher
        {
            get { return UIThreadDispatcher.Current.Dispatcher;}
            set 
            {
                if (value == null)
                {
                    throw new ArgumentNullException("UIDispatcher");
                } 
                UIThreadDispatcher.Current.Dispatcher = value;
            }
        }
        
        public HealthVaultApp(HealthVaultAppSettings appSettings)
        {
            m_lock = new object();

            m_appSettings = appSettings;
            m_serviceInfo = new ServiceInfoProxy()
            {
                ShellUrl = appSettings.ShellUrl,
                ServiceUrl = appSettings.ServiceUrl
            };
            m_startupStatus = AppStartupStatus.Cancelled;
            m_appInfo = new AppInfoProxy(appSettings.MasterAppId, appSettings.IsMultiInstanceAware);

            m_client = new HealthVaultClient(
                m_appInfo,
                m_serviceInfo,
                appSettings.IsFirstParty,
                appSettings.WebAuthorizer != null ? (IWebAuthorizer)appSettings.WebAuthorizer : null);

            if (appSettings.IsFirstParty)
            {
                m_liveIdHostName = appSettings.LiveIdHostName;
                m_client.LiveIdHostName = appSettings.LiveIdHostName;
            }

            m_localVault = new LocalVault(this, appSettings.Folder, appSettings.Folder);
            m_vocabs = new Vocabs(this);
            
            UIThreadDispatcher.Current.Init();
        }

        public AppStartupStatus StartupStatus
        {
            get { return m_startupStatus; }
        }

        public bool DebugMode
        {
            get { return m_client.Debug; }
            set { m_client.Debug = value; }
        }

        public IAppInfo AppInfo
        {
            get { return m_appInfo; }
        }

        public IServiceInfo ServiceInfo
        {
            get { return m_serviceInfo; }
            set 
            {
                lock (m_lock)
                {
                    if (value == null)
                    {
                        throw new ArgumentNullException("ServiceInfo");
                    }

                    m_serviceInfo = (ServiceInfoProxy)value;
                    m_client.ServiceInfo = m_serviceInfo;
                }
            }
        }

        public IServiceDefinition ServiceDefinition { get { return m_serviceDefinition; } }

        /// <summary>
        ///     An app could have been created, but need not have authorized records
        /// </summary>
        public bool IsAppCreated
        {
            get { return m_client.IsProvisioned; }
        }

        /// <summary>
        ///     An app is only truly provisioned if it is both created and has authorized records
        /// </summary>
        public bool IsAppProvisioned
        {
            get { return (m_client.IsProvisioned && HasAuthorizedRecords); }
        }

        public UserInfo UserInfo
        {
            get
            {
                lock (m_lock)
                {
                    return m_user;
                }
            }
            set
            {
                lock (m_lock)
                {
                    if (m_user != null)
                    {
                        m_user.SetClient(null);
                    }

                    m_user = value;
                    if (m_user != null)
                    {
                        m_user.SetClient(m_client);
                    }
                }
            }
        }

        public bool HasAuthorizedRecords
        {
            get
            {
                UserInfo userInfo = UserInfo;
                return (userInfo != null && userInfo.HasAuthorizedRecords);
            }
        }

        public LocalVault LocalVault
        {
            get { return m_localVault; }
        }

        public Vocabs Vocabs
        {
            get { return m_vocabs; }
        }

        public bool HasUserInfo
        {
            get { return (UserInfo != null); }
        }

        internal HealthVaultClient Client
        {
            get { return m_client; }
        }

        public UserIdentity UserIdentity
        {
            get { return m_client.UserIdentity; }
        }

        public ServerErrorNumber ServerErrorNumber
        {
            get { return m_serverErrorNumber; }
        }

        public string LiveIdHostName
        {
            get { return m_liveIdHostName; }
        }

        public static bool HasInternetAccess()
        {
            ConnectionProfile profile = NetworkInformation.GetInternetConnectionProfile();
            return (profile != null && profile.GetNetworkConnectivityLevel() == NetworkConnectivityLevel.InternetAccess);
        }

        public void HandledServerError()
        {
            m_serverErrorNumber = 0;
        }

        public IAsyncAction StartAsync()
        {
            m_startupStatus = AppStartupStatus.Pending;
            return AsyncInfo.Run(EnsureProvisionedAsync);
        }

        public IAsyncAction ResetAsync()
        {
            m_client.ResetState();

            return AsyncInfo.Run(cancelToken => ResetUserInfoAsync(cancelToken));
        }

        public IAsyncAction ResetUserInfoAsync()
        {
            m_client.State.OnlineToken = null;

            if (m_client.State.Credentials != null)
            {
                m_client.State.Credentials.Clear();
            }

            return AsyncInfo.Run(cancelToken => ResetUserInfoAsync(cancelToken));
        }

        public IAsyncOperation<bool> IsAuthorizedOnServerAsync()
        {
            return AsyncInfo.Run(cancelToken => m_client.IsAppAuthorizedOnServerAsync(cancelToken));
        }

        public IAsyncOperation<IsValidHealthVaultAccountResponse> HasAccountAsync()
        {
            return AsyncInfo.Run(
                async cancelToken => { 
                    try {
                            return await m_client.IsValidHealthVaultAccount<IsValidHealthVaultAccountResponse>(
                                    cancelToken);
                        }
                        catch (TaskCanceledException)
                        {
                            m_startupStatus = AppStartupStatus.Cancelled;
                            throw;
                        }
                    });
        }

        public IAsyncOperation<bool> CreateAccountAsync(CreateAccountPersonInfo createAccountPersonInfo)
        {
            return AsyncInfo.Run(
                async cancelToken => {
                    try
                    {
                        return await CreateAccountAsync(
                            createAccountPersonInfo,
                            cancelToken);
                    }
                    catch (TaskCanceledException)
                    {
                        m_startupStatus = AppStartupStatus.Cancelled;
                        throw;
                    }
                });
        }

        public IAsyncOperation<CreateRecordResponse> CreateRecordAsync(CreateRecordParams createRecordParams)
        {
            return AsyncInfo.Run(cancelToken => m_client.CreateRecordAsync<CreateRecordResponse>(createRecordParams, cancelToken));
        }

        public IAsyncOperation<WebAuthenticationStatus> AuthorizeAdditionalRecordsAsync()
        {
            return AsyncInfo.Run(
                async cancelToken =>
                      {
                          AuthResult result =
                              await m_client.Shell.AppAuthAsync().AsAsyncOperation().AsTask(cancelToken);
                          if (result.Status == WebAuthenticationStatus.Success)
                          {
                              await UpdateUserInfoAsync().AsTask(cancelToken);
                          }

                          return result.Status;
                      }
                );
        }

        public IAsyncAction UpdateUserInfoAsync()
        {
            if (m_startupStatus != AppStartupStatus.Success)
            {
                throw new InvalidOperationException("App not started");
            }

            return AsyncInfo.Run(cancelToken => UpdateUserInfoAsync(cancelToken));
        }

        public IAsyncAction LoadServiceDefinitionAsync()
        {
            return AsyncInfo.Run(LoadServiceDefinitionAsync);
        }

        // This will go online...
        public IAsyncOperation<IList<IRecord>> GetRecordsUpdatedOnServerSinceDate(DateTimeOffset updateDate)
        {
            return AsyncInfo.Run<IList<IRecord>>(async cancelToken => 
            {
                UpdatedRecord[] updatedRecords = await m_client.ServiceMethods.GetRecordsUpdatedSinceDate(updateDate, cancelToken);
                if (updatedRecords.IsNullOrEmpty())
                {
                    return null;
                }

                UserInfo userInfo = this.UserInfo;
                var records = (
                    from record in 
                        (
                        from updatedRecord in updatedRecords
                        select userInfo.GetRecord(updatedRecord.RecordID)
                        )
                    where record != null
                    select record
                ).ToArray();

                return records;
            });
        }

        public IAsyncOperation<SelectInstanceResponse> SelectInstanceAsync(
            HealthVault.Types.Location location)
        {
            if (location == null)
            {
                throw new ArgumentNullException("location");
            }

            return AsyncInfo.Run(cancelToken =>
                m_client.ServiceMethods.SelectInstanceAsync<SelectInstanceResponse>(location, cancelToken));
        }

        private async Task EnsureProvisionedAsync(CancellationToken cancelToken)
        {
            m_startupStatus = AppStartupStatus.Pending;

            if (m_client.UseOnlineAuthModel)
            {
               m_startupStatus = await ProvisionAppForOnlineAuthModel(cancelToken);
            }
            else
            {
                m_startupStatus = await ProvisionAppForSODAAuthModel(cancelToken);
            }
        }

        private async Task<AppStartupStatus> ProvisionAppForOnlineAuthModel(CancellationToken cancelToken)
        {
            PersonInfo personInfo = null;

            try
            {
                personInfo = await BatchCallsForOnlinePersonInfoAsync(cancelToken);
            }
            catch (ServerException ex)
            {
                if (ex.IsStatusCode(ServerStatusCode.CredentialNotFound))
                {
                    m_startupStatus = AppStartupStatus.CredentialNotFound;
                }

                throw;
            }
            catch (TaskCanceledException)
            {
                m_startupStatus = AppStartupStatus.Cancelled;
                throw;
            }

            HandleEncryptionSetCache();

            UserInfo = personInfo != null ? new UserInfo(personInfo) : null;
            
            m_localVault.RecordStores.ResetRecordObjects(UserInfo);
            await SaveUserInfoAsync(cancelToken);

            return AppStartupStatus.Success;
        }

        private async Task<AppStartupStatus> ProvisionAppForSODAAuthModel(CancellationToken cancelToken)
        {
            bool isNewApp = !IsAppCreated;
            AppStartupStatus appStartupStatus = AppStartupStatus.Pending;

            try
            {
                appStartupStatus = await EnsureAppCreatedAsync(cancelToken);
            }
            catch
            {
                appStartupStatus = AppStartupStatus.Failed;
                throw;
            }

            if (appStartupStatus != AppStartupStatus.Success)
            {
                await SetUserAndSaveAsync(null, cancelToken);
                return appStartupStatus;
            }

            HandleEncryptionSetCache();

            if (!isNewApp)
            {
                await LoadUserInfoAsync(cancelToken);
            }

            if (!HasUserInfo)
            {
                //
                // Download updated Person Information
                //
                await UpdateUserInfoAsync(cancelToken);
            }

            return AppStartupStatus.Success;
        }

        private async Task<PersonInfo> BatchCallsForOnlinePersonInfoAsync(CancellationToken cancelToken)
        {
            PersonInfo personInfo = null;

            if (!await m_client.EnsureUserIdentityAsync(cancelToken, 
                HealthVaultClient.MBISSLAuthPolicy))
            {
                return personInfo;
            }

            personInfo = await m_client.BatchCallsForOnlinePersonInfoAsync(cancelToken);

            return personInfo;
        }

        private async Task<AppStartupStatus> EnsureAppCreatedAsync(CancellationToken cancelToken)
        {
            if (m_client.UseOnlineAuthModel 
                && !await m_client.EnsureUserIdentityAsync(cancelToken,
                        HealthVaultClient.HBIAuthPolicy))
            {
                return AppStartupStatus.Failed;
            }

            // See if we have a cached ServiceInfo.
            ServiceInfo cachedInfo = await HealthVault.Foundation.ServiceInfo.Load();
            if (cachedInfo != null)
            {
                UpdateServiceInfo(cachedInfo);
            }

            WebAuthenticationStatus authStatus = await m_client.EnsureAppProvisionedAsync(cancelToken);

            if (cachedInfo == null || m_client.ServiceInfo.ServiceUrl != ServiceInfo.ServiceUrl)
            {
                UpdateServiceInfo(m_client.ServiceInfo);
                await m_client.ServiceInfo.Save();
            }

            return WebAuthenticationStatusToStartupStatus(authStatus);
        }

        private void UpdateServiceInfo(ServiceInfo serviceInfo)
        {
            // Setting ServiceInfo updates m_client.ServiceInfo too
            ServiceInfo = new ServiceInfoProxy()
            {
                ServiceUrl = serviceInfo.ServiceUrl,
                ShellUrl = serviceInfo.ShellUrl
            };

            m_appSettings.ServiceUrl = ServiceInfo.ServiceUrl;
            m_appSettings.ShellUrl = ServiceInfo.ShellUrl;
        }

        private async Task UpdateUserInfoAsync(CancellationToken cancelToken)
        {
            PersonInfo person = null;

            if (m_client.UseOnlineAuthModel)
            {
                person = await BatchCallsForOnlinePersonInfoAsync(cancelToken);
            }
            else
            {
                PersonInfo[] persons = await m_client.ServiceMethods.GetAuthorizedPersonsAsync(cancelToken);
                if (!persons.IsNullOrEmpty())
                {
                    person = persons[0];
                }
            }

            UserInfo = person != null ?
                new UserInfo(person) :
                null;

            if (UserInfo != null)
            {
                m_localVault.RecordStores.ResetRecordObjects(UserInfo);
            }

            await SaveUserInfoAsync(cancelToken);
        }

        private async Task SetUserAndSaveAsync(UserInfo userInfo, CancellationToken cancelToken)
        {
            UserInfo = userInfo;
            await SaveUserInfoAsync(cancelToken);
        }

        private async Task SaveUserInfoAsync(CancellationToken cancelToken)
        {
            string xml = UserInfo != null ? UserInfo.Serialize() : null;
            await m_localVault.RecordRoot.PutAsync(UserInfoKey, xml);
        }

        private async Task LoadUserInfoAsync(CancellationToken cancelToken)
        {
            try
            {
                var xml = (string) await m_localVault.RecordRoot.GetAsync(UserInfoKey, typeof (string));
                if (!string.IsNullOrEmpty(xml))
                {
                    UserInfo = UserInfo.Deserialize(xml);
                    return;
                }
            }
            catch
            {
            }

            UserInfo = null;
        }

        private async Task ResetUserInfoAsync(CancellationToken cancelToken)
        {
            await m_localVault.RecordRoot.DeleteAsync(UserInfoKey);
            UserInfo = null;
        }

        private AppStartupStatus WebAuthenticationStatusToStartupStatus(WebAuthenticationStatus authStatus)
        {
            switch (authStatus)
            {
                default:
                    return AppStartupStatus.Failed;

                case WebAuthenticationStatus.Success:
                    return AppStartupStatus.Success;

                case WebAuthenticationStatus.UserCancel:
                    return AppStartupStatus.Cancelled;
            }
        }

        private async Task LoadServiceDefinitionAsync(CancellationToken cancelToken)
        {
            var getServiceDefinitionResponse = await m_client.ServiceMethods.GetServiceDefinition(
                new ServiceDefinitionResponseSections[] { ServiceDefinitionResponseSections.Platform },
                cancelToken);

            ConfigurationEntry[] entires = getServiceDefinitionResponse.Platform.ConfigurationEntries;

            ConfigurationEntry liveIdAuthPolicyConfig = entires.FirstOrDefault(configEntry => configEntry.Key.Equals("liveIdAuthPolicy"));

            m_serviceDefinition = new ServiceDefinitionProxy {LiveIdAuthPolicy = liveIdAuthPolicyConfig.Value};
        }

        private async Task<bool> CreateAccountAsync(
           CreateAccountPersonInfo createAccountPersonInfo,
           CancellationToken cancelToken)
        {
            try
            {
                return await m_client.CreateAccountAsync(createAccountPersonInfo, cancelToken);
            }
            catch (ServerException se)
            {
                switch (se.StatusCode)
                {
                    case (int)ServerStatusCode.InvalidAge:
                        m_serverErrorNumber = ServerErrorNumber.InvalidAge;
                        break;
                    case (int)ServerStatusCode.InvalidIPAddress:
                        m_serverErrorNumber = ServerErrorNumber.InvalidIPAddress;
                        break;
                    case (int)ServerStatusCode.InvalidPerson:
                        m_serverErrorNumber = ServerErrorNumber.InvalidPerson;
                        break;
                    case (int)ServerStatusCode.DuplicateCredentialFound:
                        m_serverErrorNumber = ServerErrorNumber.DuplicateCredentialFound;
                        break;
                }

                throw;
            }
        }

        private void HandleEncryptionSetCache()
        {
            // Set up the encrypted local vault
            if (m_appSettings.UseEncryption)
            {
                var encryptedStore = new EncryptedObjectStore(
                    FolderObjectStore.CreateRoot(m_appSettings.Folder),
                    Client.Cryptographer,
                    Client.State.ProvisioningInfo.SharedSecret);

                m_localVault = new LocalVault(
                    this,
                    FolderObjectStore.CreateRoot(m_appSettings.Folder),
                    encryptedStore);
            }

            // Set the cache setting
            m_localVault.RecordStores.MaxCachedItems = m_appSettings.MaxCachedItems;
        }
    }
}