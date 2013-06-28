// (c) Microsoft. All rights reserved

using System;

namespace HealthVault.Foundation
{
    public interface IAppInfo
    {
        string AppId { get; set; }

        string InstanceName { get; set; }

        string CultureCode { get; set; }

        bool IsMultiInstanceAware { get; set; }
    }

    internal class AppInfoProxy : AppInfo, IAppInfo
    {
        private string m_appId;

        internal AppInfoProxy(string masterAppId)
            : this(masterAppId, false)
        {
        }

        internal AppInfoProxy(string masterAppId, bool isMultiInstanceAware)
        {
            masterAppId.ValidateRequired("masterAppId");
            AppId = masterAppId;
            IsMultiInstanceAware = isMultiInstanceAware;
        }

        #region IAppInfo Members

        public string AppId
        {
            get
            {
                if (m_appId == null)
                {
                    m_appId = MasterAppId.ToString("D");
                }
                return m_appId;
            }
            set
            {
                MasterAppId = Guid.Parse(value);
                m_appId = null;
            }
        }

        #endregion
    }
}