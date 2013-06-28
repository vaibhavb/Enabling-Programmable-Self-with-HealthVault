// (c) Microsoft. All rights reserved

using System;
using System.Collections.Generic;
using System.Linq;
using HealthVault.Foundation.Types;
using HealthVault.Types;

namespace HealthVault.Foundation
{
    public sealed class UserInfo : IHealthVaultTypeSerializable
    {
        private PersonInfo m_personInfo;
        private List<IRecord> m_records;

        internal UserInfo(PersonInfo personInfo)
        {
            personInfo.ValidateRequired("personInfo");

            ApplyPersonInfo(personInfo);
        }

        public string PersonId
        {
            get { return m_personInfo.PersonId; }
        }

        public string Name
        {
            get { return m_personInfo.Name; }
        }

        public IReadOnlyList<IRecord> AuthorizedRecords
        {
            get { return m_records; }
        }

        public bool HasAuthorizedRecords
        {
            get { return !m_records.IsNullOrEmpty(); }
        }

        public string Country
        {
            get { return m_personInfo.Location != null ? m_personInfo.Location.Country : String.Empty; }
        }

        public string State
        {
            get
            {
                if (m_personInfo.Location != null)
                {
                    return m_personInfo.Location.State ?? String.Empty;
                }

                return String.Empty;
            }
        }

        public IRecord GetRecord(string recordID)
        {
            if (string.IsNullOrEmpty(recordID))
            {
                throw new ArgumentException("recordID");
            }

            return m_records.FirstOrDefault((record) => string.Equals(record.ID, recordID, StringComparison.Ordinal));
        }

        #region IHealthVaultTypeSerializable Members

        public string Serialize()
        {
            return m_personInfo.ToXml();
        }

        public void Validate()
        {
            m_personInfo.ValidateRequired("User");
        }

        #endregion

        public static UserInfo Deserialize(string xml)
        {
            var person = HealthVaultClient.Serializer.FromXml<PersonInfo>(xml);
            if (person == null)
            {
                throw new ArgumentException("xml");
            }

            return new UserInfo(person);
        }

        internal void SetClient(HealthVaultClient client)
        {
            if (m_records == null)
            {
                return;
            }

            foreach (RecordImpl record in m_records)
            {
                record.Client = client;
            }
        }

        private void ApplyPersonInfo(PersonInfo personInfo)
        {
            m_personInfo = personInfo;

            if (!m_personInfo.HasRecords)
            {
                m_records = new List<IRecord>();
                return;
            }

            m_records = (from record in m_personInfo.Records
                select new RecordImpl(record, m_personInfo.PersonId)).ToList<IRecord>();
        }
        
        public override string ToString()
        {
            return string.Format("[{0}], {1}", PersonId, Name);
        }
    }
}