using System;

namespace EntityFrameworkCore.BulkExtensions
{
    [Flags]
    public enum Options
    {
        Default = 1,
        OutputIdentity = 2
    }
}