using EntityFramework.BulkExtensions.Commons.Helpers;

namespace EntityFrameworkCore.BulkExtensions.Extensions
{
    internal static class EnumExtensions
    {
        internal static BulkOptions ToSharedOptions(this Options option)
        {
            return (BulkOptions) (int) option;
        }
    }
}