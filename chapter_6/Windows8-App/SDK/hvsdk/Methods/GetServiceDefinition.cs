// (c) Microsoft. All rights reserved

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using HealthVault.Foundation.Types;

namespace HealthVault.Foundation.Methods
{
    public class GetServiceDefinition : HealthVaultMethod
    {
        private object[] m_data;

        public GetServiceDefinition(HealthVaultClient client)
            : base(client)
        {
            m_data = null;
        }

        public GetServiceDefinition(HealthVaultClient client, ServiceDefinitionResponseSections[] responseSections)
            : base(client)
        {
            if (responseSections == null)
            {
                throw new ArgumentNullException("responseSections");
            }

            m_data = new object[] { new ResponseConfig(responseSections) };
        }

        public GetServiceDefinition(HealthVaultClient client, DateTime lastUpdated)
            : base(client)
        {
            m_data = new object[] { new LastUpdatedDate(lastUpdated) };
        }

        public GetServiceDefinition(HealthVaultClient client, DateTime lastUpdated, ServiceDefinitionResponseSections[] responseSections)
            : base(client)
        {
            if (responseSections == null)
            {
                throw new ArgumentNullException("responseSections");
            }

            m_data = new object[] { new LastUpdatedDate(lastUpdated), new ResponseConfig(responseSections) };
        }

        public override Request CreateRequest()
        {
            var request = new Request("GetServiceDefinition", 2);
            request.Body.Data = m_data;
            request.IsAnonymous = true;
            request.Header.AppId = this.Client.AppInfo.MasterAppId;

            return request;
        }

        public override Task<Response> ExecuteAsync(CancellationToken cancelToken)
        {
            Request request = CreateRequest();
            return Client.ExecuteRequestAsync(request, typeof(GetServiceDefinitionResponse), cancelToken);
        }
    }

    [XmlType("updated-date")]
    public class LastUpdatedDate
    {
        public LastUpdatedDate()
        {
        }

        public LastUpdatedDate(DateTime lastUpdated)
        {
            LastUpdated = lastUpdated;
        }

        [XmlText]
        public DateTime LastUpdated { get; set; }
    }

    [XmlType("response-sections")]
    public class ResponseConfig
    {
        public ResponseConfig()
        {
        }

        public ResponseConfig(ServiceDefinitionResponseSections[] responseSections)
        {
            ResponseSections = new string[responseSections.Length];

            for (var i = 0; i < responseSections.Length; i++)
            {
                if (responseSections[i] == ServiceDefinitionResponseSections.XmlOverHttpMethods)
                {
                    ResponseSections[i] = "xml-over-http-methods";
                }
                else
                {
                    ResponseSections[i] = responseSections[i].ToString().ToLowerInvariant();
                }
            }
        }

        [XmlElement("section")]
        public string[] ResponseSections;
    }
}