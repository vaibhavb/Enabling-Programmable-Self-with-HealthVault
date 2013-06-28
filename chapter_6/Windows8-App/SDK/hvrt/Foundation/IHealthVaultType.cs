using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HealthVault.Foundation;

namespace HealthVault.Foundation
{
    public interface IHealthVaultType
    {
        string Serialize();
        void Validate(); 
    }
}
