using System;

namespace EntityFramework.BulkOperations
{
    [Flags]
    public enum BulkOptions
    {
        Default = 1,
        OutputIdentity = 2,
        OutputComputed = 3,
    }
}