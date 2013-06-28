// (c) Microsoft. All rights reserved

namespace HealthVault.Foundation
{
    /// <summary>
    /// Represent Server Status Codes as HRESULTS, for propagation to Javascript
    /// and other WinRT users
    /// </summary>
    public enum ServerErrorNumber : uint
    {
        Failed = HResults.ServerErrorBase + ServerStatusCode.Failed,
        BadHttp = HResults.ServerErrorBase + ServerStatusCode.BadHttp,
        InvalidXml = HResults.ServerErrorBase + ServerStatusCode.InvalidXml,
        InvalidRequestIntegrity = HResults.ServerErrorBase + ServerStatusCode.InvalidRequestIntegrity,
        BadMethod = HResults.ServerErrorBase + ServerStatusCode.BadMethod,
        InvalidApp = HResults.ServerErrorBase + ServerStatusCode.InvalidApp,
        CredentialTokenExpired = HResults.ServerErrorBase + ServerStatusCode.CredentialTokenExpired,
        InvalidToken = HResults.ServerErrorBase + ServerStatusCode.InvalidToken,
        InvalidPerson = HResults.ServerErrorBase + ServerStatusCode.InvalidPerson,
        InvalidRecord = HResults.ServerErrorBase + ServerStatusCode.InvalidRecord,
        AccessDenied = HResults.ServerErrorBase + ServerStatusCode.AccessDenied,
        InvalidItem = HResults.ServerErrorBase + ServerStatusCode.InvalidItem,
        InvalidFilter = HResults.ServerErrorBase + ServerStatusCode.InvalidFilter,
        TypeIDNotFound = HResults.ServerErrorBase + ServerStatusCode.TypeIdNotFound,
        CredentialNotFound = HResults.ServerErrorBase + ServerStatusCode.CredentialNotFound,
        DuplicateCredentialFound = HResults.ServerErrorBase + ServerStatusCode.DuplicateCredentialFound,
        InvalidRecordState = HResults.ServerErrorBase + ServerStatusCode.InvalidRecordState,
        RequestTimedOut = HResults.ServerErrorBase + ServerStatusCode.RequestTimedOut,
        VocabNotFound = HResults.ServerErrorBase + ServerStatusCode.VocabNotFound,
        VersionStampMismatch = HResults.ServerErrorBase + ServerStatusCode.VersionStampMismatch,
        AuthenticatedSessionTokenExpired = HResults.ServerErrorBase + ServerStatusCode.AuthenticatedSessionTokenExpired,
        RecordQuotaExceeded = HResults.ServerErrorBase + ServerStatusCode.RecordQuotaExceeded,
        ApplicationLimitExceeded = HResults.ServerErrorBase + ServerStatusCode.ApplicationLimitExceeded,
        VocabAccessDenied = HResults.ServerErrorBase + ServerStatusCode.VocabAccessDenied,
        InvalidAge = HResults.ServerErrorBase + ServerStatusCode.InvalidAge,
        InvalidIPAddress = HResults.ServerErrorBase + ServerStatusCode.InvalidIPAddress
    }
}