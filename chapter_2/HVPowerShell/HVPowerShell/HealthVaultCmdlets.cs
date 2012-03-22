//
// Copyright 2011 Vitraag LLC.
//
// Author : Vaibhav Bhandari (vaibhavb@vitraag.com)
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Management.Automation;
using System.Data.Common;
using System.Linq;
using System.Xml.Linq;

using Microsoft.Health;
using Microsoft.Health.ItemTypes;
using System.Net;

namespace HealthVault.PowerShell.Cmdlets
{
    
    [Cmdlet("Grant", "HVaccess")]
    public class GrantHVaccess : PSCmdlet
    {
        private Guid HVPowerShellGuid = new Guid("9aa44e59-faa9-417e-b419-0e627c0d1b91");
        private Uri HVPlatform = new Uri("https://platform.healthvault-ppe.com/platform/");
        private Uri HVShell = new Uri("https://account.healthvault-ppe.com/");

        protected override void ProcessRecord()
        {
            HealthClientApplication clientApp = HealthClientApplication.Create(
                Guid.NewGuid(), HVPowerShellGuid, HVShell, HVPlatform);

            // Verify the application instance.
            //   Create a new instance if necessary.

            if (clientApp.GetApplicationInfo() == null)
            {
                // Create a new client instance.                  
                clientApp.StartApplicationCreationProcess();

                // A new client instance always requests authorization from the 
                //   current user using the default browser.

                // Wait for the user to return from the shell.
                if (ShouldContinue("Is Auth done - (Y)?", "Is auth done?", 
                             ref _yesToAll, ref _noToAll))
                {
                    // Store the SODA client details                    
                    StringBuilder data = new StringBuilder();
                    data.Append(clientApp.ApplicationId.ToString());
                    using (StreamWriter sw = new StreamWriter(HvShellUtilities.GetClientAppAuthFileNameFullPath()))
                    {
                        sw.Write((data));
                    }                    
                    List<PersonInfo> authorizedPeople = 
                        new List<PersonInfo>(clientApp.ApplicationConnection.GetAuthorizedPeople());
                    WriteObject(authorizedPeople);
                }
            }
        }
        private bool _yesToAll, _noToAll;
    }

    [Cmdlet("Get", "Personinfo")]
    public class GetPersonInfo : PSCmdlet
    {

        protected override void ProcessRecord()
        {
            HealthClientApplication clientApp = HvShellUtilities.GetClient();
            if (clientApp.GetApplicationInfo() != null)
            {
                List<PersonInfo> authorizedPeople =
                        new List<PersonInfo>(clientApp.ApplicationConnection.GetAuthorizedPeople());
                WriteObject(authorizedPeople);
            }
        }
    }

    [Cmdlet("Get", "Things")]
    public class GetThing: PSCmdlet
    {
        [Parameter(Position = 0)]
        [ValidateNotNullOrEmpty]
        public string Type
        {
            get { return _type; }
            set { _type = value; }
        }

        private string _type;

        protected override void ProcessRecord()
        {
            HealthClientApplication clientApp = HvShellUtilities.GetClient();

            List<PersonInfo> authorizedPeople = new List<PersonInfo>(clientApp.ApplicationConnection.GetAuthorizedPeople());

            // Create an authorized connection for each person on the 
            //   list.             
            HealthClientAuthorizedConnection authConnection = clientApp.CreateAuthorizedConnection(
                authorizedPeople[0].PersonId);
               
            // Use the authorized connection to read the user's default 
            //   health record.
            HealthRecordAccessor access = new HealthRecordAccessor(
                authConnection, authConnection.GetPersonInfo().GetSelfRecord().Id);

            // Search the health record for basic demographic 
            //   information.
            //   Most user records contain an item of this type.
            HealthRecordSearcher search = access.CreateSearcher();
            HealthRecordFilter filter = new HealthRecordFilter(
                HvShellUtilities.NameToTypeId(Type));
            
            search.Filters.Add(filter);
            
            foreach (Object o in search.GetMatchingItems()[0])
            {
                WriteObject(o);
            }
        }
    }

    /// <summary>
    /// Get HealthVault Service Definition
    /// </summary>
    [Cmdlet("Get", "ServiceDefinition")]
    public class GetServiceDefinition: PSCmdlet
    {
        protected override void ProcessRecord()
        {
            HealthClientApplication clientApp = HvShellUtilities.GetClient();
            ServiceInfo s = clientApp.ApplicationConnection.GetServiceDefinition();
            WriteObject(s);
        }
    }


    /// <summary>
    /// Get all HealthVault Thing-types
    /// </summary>
    [Cmdlet("Get", "ThingType")]
    public class GetThingType : PSCmdlet
    {
        [Parameter(Position = 0)]
        public string ThingType
        {
            get { return _type; }
            set { _type = value; }
        }
        private string _type;

        protected override void ProcessRecord()
        {
            if (string.IsNullOrEmpty(ThingType))
            {
                HealthClientApplication clientApp = HvShellUtilities.GetClient();
                WriteObject(clientApp.ApplicationConnection.GetVocabulary("thing-types"));
            }
            else
            {
                WriteObject(
                ItemTypeManager.GetHealthRecordItemTypeDefinition(
                    new Guid(ThingType), HvShellUtilities.GetClient().ApplicationConnection)
                    );
            }
        }


    }

    /// <summary>
    /// Get all HealthVault Vocabularies
    /// </summary>
    [Cmdlet("Get", "Vocabulary")]
    public class GetVocabulary : PSCmdlet
    {
        protected override void ProcessRecord()
        {
            HealthClientApplication clientApp = HvShellUtilities.GetClient();
            WriteObject(clientApp.ApplicationConnection.GetVocabularyKeys());
        }
    }


    /// <summary>
    /// Currently only supports Weight.
    /// </summary>
    [Cmdlet("Add", "Things")]
    public class PutThings : PSCmdlet
    {
        [Parameter(Position = 0, Mandatory = true)]
        [ValidateNotNullOrEmpty]
        public string Type
        {
            get { return _type; }
            set { _type = value; }
        }

        private string _type;

        [Parameter(Position = 1, Mandatory = true)]
        [ValidateNotNullOrEmpty]
        public int Value
        {
            get { return _value; }
            set { _value = value; }
        }

        private int _value;

        protected override void ProcessRecord()
        {
            HealthClientApplication clientApp = HvShellUtilities.GetClient();
            List<PersonInfo> authorizedPeople = new List<PersonInfo>
                (clientApp.ApplicationConnection.GetAuthorizedPeople());
            // Create an authorized connection for each person on the 
            //   list.             
            HealthClientAuthorizedConnection authConnection = clientApp.CreateAuthorizedConnection(
                authorizedPeople[0].PersonId);

            // Use the authorized connection to read the user's default 
            //   health record.
            HealthRecordAccessor access = new HealthRecordAccessor(
                authConnection, authConnection.GetPersonInfo().GetSelfRecord().Id);

            Weight weight = new Weight();
            weight.Value = new WeightValue(Value / 2.2, 
                new DisplayValue(Value, "pounds"));

            access.NewItem(weight);
        }
    }

    [Cmdlet("Import", "HvDataXml")]
    public class ImportHVDataXml : PSCmdlet
    {
        [Parameter(Position = 0, Mandatory = true)]
        [ValidateNotNullOrEmpty]
        public string FileName
        {
            get { return _type; }
            set { _type = value; }
        }
        private string _type;

        protected override void ProcessRecord()
        {
            HealthClientApplication clientApp = HvShellUtilities.GetClient();
            List<PersonInfo> authorizedPeople = new List<PersonInfo>
                (clientApp.ApplicationConnection.GetAuthorizedPeople());

            // Create an authorized connection for each person on the 
            //   list.             
            HealthClientAuthorizedConnection authConnection = clientApp.CreateAuthorizedConnection(
                authorizedPeople[0].PersonId);

            // Use the authorized connection to read the user's default 
            //   health record.
            HealthRecordAccessor accessor = new HealthRecordAccessor(
                authConnection, authConnection.GetPersonInfo().GetSelfRecord().Id);

            HealthServiceRequest request =
               new HealthServiceRequest(accessor.Connection, "PutThings", 2, accessor);

            // Read the input file
            request.Parameters = System.IO.File.ReadAllText(Path.GetFullPath(FileName));
            request.Execute();
        }

    }

    [Cmdlet("Get", "Devices")]
    public class GetDevices : PSCmdlet
    {
        protected override void ProcessRecord()
        {
            WebClient webClient = new WebClient();
            string deviceList = System.Text.Encoding.UTF8.GetString(
                webClient.DownloadData(HvShellUtilities.DeviceDirectoryUrl)
                );
            XElement xml = XElement.Parse(deviceList);            
            WriteObject(xml);
        }
    }

    [Cmdlet("Get", "Applications")]
    public class GetApplications : PSCmdlet
    {
        protected override void ProcessRecord()
        {
            WebClient webClient = new WebClient();
            string appList = System.Text.Encoding.UTF8.GetString(
                webClient.DownloadData(HvShellUtilities.ApplicationDirectoryUrl)
                );
            XElement xml = XElement.Parse(appList);
            WriteObject(xml);
        }
    }
}

public class HvShellUtilities
{

    private static Guid _HVPowerShellGuid = new Guid("9aa44e59-faa9-417e-b419-0e627c0d1b91");
    private static Uri _HVPlatform = new Uri("https://platform.healthvault-ppe.com/platform/");
    private static Uri _HVShell = new Uri("https://account.healthvault-ppe.com/");
    public static string DeviceDirectoryUrl = "https://platform.healthvault.com/platform/directory/devices";
    public static string ApplicationDirectoryUrl = "https://platform.healthvault.com/platform/directory/applications";

    public static string GetClientAppAuthFileNameFullPath()
    {
        // Write information to local store for this application
        string docPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string fileName = "application" + ".hv" + ".txt";
        return docPath + @"." + fileName;
    }

    public static Guid GetClientId()
    {
        string s;
        using (StreamReader r = new StreamReader(new FileStream(GetClientAppAuthFileNameFullPath(), FileMode.Open)))
        {
            s = r.ReadToEnd();
        }
        return new Guid(s);
    }

    public static HealthClientApplication GetClient()
    {
        HealthClientApplication clientApp = HealthClientApplication.Create(
            HvShellUtilities.GetClientId(), _HVPowerShellGuid, _HVShell, _HVPlatform);
        return clientApp;
    }

    public static Guid NameToTypeId(string name)
    {
        switch (name)
        {
            case "weight":
                return Weight.TypeId;
            case "sleep":
                return SleepJournalAM.TypeId;
            case "bp":
                return BloodPressure.TypeId;
            case "bg":
                return BloodGlucose.TypeId;
            case "exercise":
                return Exercise.TypeId;
            case "basic":
                return Basic.TypeId;
            default:
                return BasicV2.TypeId;
        }
    }
}

