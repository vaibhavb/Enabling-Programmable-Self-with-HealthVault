// (c) Microsoft. All rights reserved

namespace HealthVault.Foundation
{
    //
    // The great standard .NET delegates cannot be used as is - since they are not WinRT types
    // So we redeclare the most useful ones here
    //
    public delegate void ActionDelegate();

    public delegate bool PredicateDelegate(object objToTest);

    public delegate void NotifyDelegate(object sender);

    public delegate object FactoryDelegate(object key);

    public delegate void ItemChangedDelegate(object sender, string key);

    public delegate void CompletionDelegate(object sender, object result);
}