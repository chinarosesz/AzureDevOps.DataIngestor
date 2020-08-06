using System.Collections.Generic;

namespace EntityFramework.BulkExtensions.Commons.Mapping
{
    public interface IEntityMapping
    {
        string TableName { get; }
        string Schema { get; }
        IEnumerable<IPropertyMapping> Properties { get; }

        IEnumerable<IPropertyMapping> Pks { get; }

        string FullTableName { get; }

        Dictionary<string, string> HierarchyMapping { get; }
    }
}