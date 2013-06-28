// (c) Microsoft. All rights reserved

using System;
using System.Xml.Serialization;
using HealthVault.Foundation;
using Windows.Foundation;
using Windows.Storage.Streams;
using Windows.System;

namespace HealthVault.Types
{
    public sealed class Blob : IHealthVaultType
    {
        public Blob()
        {
            Url = String.Empty;
            LegacyEncoding = String.Empty;
            Encoding = String.Empty;
        }

        public Blob(BlobInfo info, int length, string url) : this()
        {
            Info = info;
            Length = length;
            Url = url;
        }

        [XmlElement("blob-info", Order = 1)]
        public BlobInfo Info { get; set; }

        [XmlElement("content-length", Order = 2)]
        public int Length { get; set; }

        [XmlElement("blob-ref-url", Order = 3)]
        public string Url { get; set; }

        [XmlElement("legacy-content-encoding", Order = 4)]
        public string LegacyEncoding { get; set; }

        [XmlElement("current-content-encoding", Order = 5)]
        public string Encoding { get; set; }

        [XmlIgnore]
        public string Name
        {
            get { return Info != null ? Info.Name : string.Empty; }
        }

        #region IHealthVaultType Members

        public void Validate()
        {
            Info.ValidateRequired("Info");
            Url.ValidateRequired("Url");
        }

        #endregion

        /// <summary>
        /// Launches the Blob using the registered Url display app
        /// </summary>
        public IAsyncOperation<bool> DisplayAsync()
        {
            Validate();

            var options = new LauncherOptions();
            options.ContentType = Info.ContentType;
            var uri = new Uri(Url);

            return Launcher.LaunchUriAsync(uri, options);
        }

        public IAsyncAction DownloadAsync(IRecord record, IOutputStream destination)
        {
            if (record == null)
            {
                throw new ArgumentNullException("record");
            }

            return record.DownloadBlob(this, destination);
        }

        public bool ShouldSerializeUrl()
        {
            return !String.IsNullOrEmpty(Url);
        }

        public bool ShouldSerializeLegacyEncoding()
        {
            return !String.IsNullOrEmpty(LegacyEncoding);
        }

        public bool ShouldSerializeEncoding()
        {
            return !String.IsNullOrEmpty(Encoding);
        }
    }
}