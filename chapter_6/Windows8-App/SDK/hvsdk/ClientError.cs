// (c) Microsoft. All rights reserved
namespace HealthVault.Foundation
{
    public enum ClientError
    {
        None = 0,
        Unexpected = 1,
        NoProvisioningInfo = 2,
        NoCredentials = 3,
        AppNotProvisioned = 4,
        StreamTooLarge = 5,
        KeysNotFound = 6,
        NoRecordSpecified = 7,
        InvalidOrMissingInstanceId = 8
    }
}