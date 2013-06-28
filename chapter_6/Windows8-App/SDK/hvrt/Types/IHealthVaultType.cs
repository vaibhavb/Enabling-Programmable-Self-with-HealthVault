// (c) Microsoft. All rights reserved

namespace HealthVault.Types
{
    public interface IHealthVaultType
    {
        void Validate();
    }

    public interface IHealthVaultTypeSerializable : IHealthVaultType
    {
        string Serialize();
    }
}