// (c) Microsoft. All rights reserved

namespace HealthVault.Foundation
{
    public enum ClientErrorNumber : uint
    {
        Unexpected = HResults.ClientErrorBase + ClientError.Unexpected,
        NoProvisioningInfo = HResults.ClientErrorBase + ClientError.NoProvisioningInfo,
        NoCredentials = HResults.ClientErrorBase + ClientError.NoCredentials,
        AppNotProvisioned = HResults.ClientErrorBase + ClientError.AppNotProvisioned,
        StreamTooLarge = HResults.ClientErrorBase + ClientError.StreamTooLarge,
        KeysNotFound = HResults.ClientErrorBase + ClientError.KeysNotFound,
        NoRecordSpecified = HResults.ClientErrorBase + ClientError.NoRecordSpecified,
        InvalidOrMissingInstanceId = HResults.ClientErrorBase + ClientError.InvalidOrMissingInstanceId,
    }
}