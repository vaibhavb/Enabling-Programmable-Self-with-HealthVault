// (c) Microsoft. All rights reserved

namespace HealthVault.Types
{
    public enum ItemSectionType
    {
        None = 0,
        //
        // Return Data
        //
        Data = 0x01,
        //
        // Standard Item Type information
        //
        Core = 0x02,
        Audits = 0x04,
        Tags = 0x08,
        Blobs = 0x10,
        EffectivePermissions = 0x20,
        Signatures = 0x40,
        //
        // What people typically want to retrieve
        //
        Standard = Data | Core
    }

    public static class ItemSections
    {
        public static string Core
        {
            get { return "core"; }
        }

        public static string Audits
        {
            get { return "audits"; }
        }

        public static string Blobs
        {
            get { return "blobpayload"; }
        }

        public static string Tags
        {
            get { return "tags"; }
        }

        public static string Permissions
        {
            get { return "effectivepermissions"; }
        }

        public static string Signatures
        {
            get { return "digitalsignatures"; }
        }
    }
}