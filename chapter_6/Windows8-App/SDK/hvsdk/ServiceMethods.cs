// (c) Microsoft. All rights reserved
using System;
using System.Threading;
using System.Threading.Tasks;
using HealthVault.Foundation.Methods;
using HealthVault.Foundation.Types;
using Windows.Security.Authentication.OnlineId;
using System.Xml.Serialization;
using System.IO;
using System.Xml;
using System.Text;

namespace HealthVault.Foundation
{
    public class ServiceMethods
    {
        private readonly HealthVaultClient m_client;

        public ServiceMethods(HealthVaultClient client)
        {
            if (client == null)
            {
                throw new ArgumentNullException("client");
            }

            m_client = client;
        }

        public async Task<AppProvisioningInfo> GetAppProvisioningInfoAsync(CancellationToken cancelToken)
        {
            var method = new NewApplicationProvisioningInfo(m_client);
            Response response = await method.ExecuteAsync(cancelToken);
            return (AppProvisioningInfo) response.GetResult();
        }

        public async Task<AppProvisioningInfo> CreateApplicationAsync(OnlineIdServiceTicket ticket, AppInfo appInfo, CancellationToken cancelToken)
        {
            if (ticket == null)
            {
                throw new ArgumentNullException("ticket");
            }

            if (appInfo == null)
            {
                throw new ArgumentNullException("appInfo");
            }

            var method = new CreateApplicationWithTicket(m_client, ticket.Value, appInfo.InstanceName);
            Response response = await method.ExecuteAsync(cancelToken);
            return (AppProvisioningInfo)response.GetResult();
        }

        public async Task<SessionCredential> GetSessionTokenAsync(CancellationToken cancelToken)
        {
            var method = new CreateAuthenticatedSessionToken(m_client);
            Response response = await method.ExecuteAsync(cancelToken);
            return (SessionCredential) response.GetResult();
        }

        public async Task<CreateAuthTokensWithTicketResponse> CreateAuthTokensWithTicketAsync(
            OnlineIdServiceTicket ticket,
            CancellationToken cancelToken)
        {
            if (ticket == null)
            {
                throw new ArgumentNullException("ticket");
            }

            var method = new CreateAuthTokensWithTicket(m_client, ticket.Value);
            Response response = await method.ExecuteAsync(cancelToken);
            return (CreateAuthTokensWithTicketResponse)response.GetResult();
        }

        public async Task<CreateCredentialTokenWithTicketResponse> CreateCredentialTokenAsync(OnlineIdServiceTicket ticket, CancellationToken cancelToken)
        {
            if (ticket == null)
            {
                throw new ArgumentNullException("ticket");
            }

            var method = new CreateCredentialTokenWithTicket(m_client, ticket.Value);
            Response response = await method.ExecuteAsync(cancelToken);
            return (CreateCredentialTokenWithTicketResponse)response.GetResult();
        }

        public async Task<CreateAccountWithTicketResponse> CreateAccountAsync(OnlineIdServiceTicket ticket, object createAccountPersonInfo, CancellationToken cancelToken)
        {
            if (ticket == null)
            {
                throw new ArgumentNullException("ticket");
            }

            if (createAccountPersonInfo == null)
            {
                throw new ArgumentNullException("createAccountPersonInfo");
            }

            var method = new CreateAccountWithTicket(m_client, ticket.Value, createAccountPersonInfo);
            Response response = await method.ExecuteAsync();

            return (CreateAccountWithTicketResponse)response.GetResult();
        }

        public async Task<TResult> IsValidHealthVaultAccount<TResult>(OnlineIdServiceTicket ticket, CancellationToken cancelToken)
        {
            if (ticket == null)
            {
                throw new ArgumentNullException("ticket");
            }

            var method = new IsValidHealthVaultAccount(m_client, ticket.Value, typeof(TResult));
            Response response = await method.ExecuteAsync(cancelToken);
            return (TResult)response.GetResult();
        }

        public async Task<PersonInfo> GetPersonInfoAsync(CancellationToken cancelToken)
        {
            var method = new GetPersonInfo(m_client);
            Response response = await method.ExecuteAsync(cancelToken);
            return ((GetPersonInfoResponse) response.GetResult()).PersonInfo;
        }

        public async Task<PersonInfo[]> GetAuthorizedPersonsAsync(CancellationToken cancelToken)
        {
            var method = new GetAuthorizedPeople(m_client);
            Response response = await method.ExecuteAsync(cancelToken);
            var authPeopleResponse = (GetAuthorizedPeopleResponse) response.GetResult();
            return authPeopleResponse.HasResults ? authPeopleResponse.Results.Persons : null;
        }
        
        public async Task<UpdatedRecord[]> GetRecordsUpdatedSinceDate(DateTimeOffset date, CancellationToken cancelToken)
        {
            var method = new GetUpdatedRecordsForApplication(m_client, date);
            Response response = await method.ExecuteAsync();
            GetUpdatedRecordsForApplicationResponse updatedRecords = (GetUpdatedRecordsForApplicationResponse) response.GetResult();
            if (updatedRecords.Updates == null)
            {
                return null;
            }

            return updatedRecords.Updates.Records;
        }

        public async Task<TResult> GetThingType<TResult>(object getThingTypeParams, CancellationToken cancelToken)
        {
            if (getThingTypeParams == null)
            {
                throw new ArgumentNullException("getThingTypeParams");
            }

            var body = new RequestBody(getThingTypeParams);
            var method = new GetThingType(m_client, body, typeof (TResult));
            Response response = await method.ExecuteAsync();

            return (TResult) response.GetResult();
        }

        public async Task<GetServiceDefinitionResponse> GetServiceDefinition(CancellationToken cancelToken)
        {
            var method = new GetServiceDefinition(m_client);
            Response response = await method.ExecuteAsync(cancelToken);

            return (GetServiceDefinitionResponse)response.GetResult();
        }

        public async Task<GetServiceDefinitionResponse> GetServiceDefinition(
            DateTime lastUpdated,
            CancellationToken cancelToken)
        {
            var method = new GetServiceDefinition(m_client, lastUpdated);
            Response response = await method.ExecuteAsync(cancelToken);

            return (GetServiceDefinitionResponse)response.GetResult();
        }

        public async Task<GetServiceDefinitionResponse> GetServiceDefinition(
            ServiceDefinitionResponseSections[] responseSections,
            CancellationToken cancelToken)
        {
            if (responseSections == null)
            {
                throw new ArgumentNullException("responseSections");
            }

            var method = new GetServiceDefinition(m_client, responseSections);
            Response response = await method.ExecuteAsync(cancelToken);

            return (GetServiceDefinitionResponse)response.GetResult();
        }

        public async Task<GetServiceDefinitionResponse> GetServiceDefinition(
            DateTime lastUpdated,
            ServiceDefinitionResponseSections[] responseSections,
            CancellationToken cancelToken)
        {
            if (responseSections == null)
            {
                throw new ArgumentNullException("responseSections");
            }

            var method = new GetServiceDefinition(m_client, lastUpdated, responseSections);
            Response response = await method.ExecuteAsync(cancelToken);

            return (GetServiceDefinitionResponse)response.GetResult();
        }

        public async Task<TResult> GetVocabularies<TResult>(object getVocabParams, CancellationToken cancelToken)
        {
            if (getVocabParams == null)
            {
                throw new ArgumentNullException("getVocabParams");
            }

            var body = new RequestBody(getVocabParams);
            var method = new GetVocabulary(m_client, body, typeof (TResult));
            Response response = await method.ExecuteAsync();

            return (TResult) response.GetResult();
        }

        public async Task<TResult> SearchVocabulary<TResult>(object searchVocabParams, CancellationToken cancelToken)
        {
            if (searchVocabParams == null)
            {
                throw new ArgumentNullException("getVocabParams");
            }

            var body = new RequestBody(searchVocabParams);
            var method = new SearchVocabulary(m_client, body, typeof (TResult));
            Response response = await method.ExecuteAsync();

            return (TResult) response.GetResult();
        }

        public async Task<TResult> CreateRecordAsync<TResult>(object createRecordParams, CancellationToken cancelToken)
        {
            if (createRecordParams == null)
            {
                throw new ArgumentNullException("createRecordParams");
            }

            var requestBody = new RequestBody(createRecordParams);
            var method = new CreateRecord(m_client, requestBody, typeof(TResult));
            Response response = await method.ExecuteAsync(cancelToken);
            return (TResult)response.GetResult();
        }

        public async Task<TResult> SelectInstanceAsync<TResult>(object location, CancellationToken cancelToken)
        {
            if (location == null)
            {
                throw new ArgumentNullException("location");
            }

            var requestBody = new RequestBody(location);
            requestBody.DataSerializer = data => {
                return data.ToXml("preferred-location");
            };

            var method = new SelectInstance(m_client, requestBody, typeof(TResult));
            Response response = await method.ExecuteAsync(cancelToken);
            return (TResult)response.GetResult();
        }
    }
}