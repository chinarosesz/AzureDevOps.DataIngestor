using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EntityFramework.BulkExtensions.Commons.Helpers;
using EntityFramework.BulkExtensions.Commons.Mapping;
using Shared.Helpers;

namespace EntityFramework.BulkExtensions.Commons.Extensions
{
    internal static class DataReaderExtension
    {
        internal static EnumerableDataReader ToDataReader<TEntity>(this IEnumerable<TEntity> entities, IEntityMapping mapping,
            OperationType operationType) where TEntity : class
        {
            var tableColumns = mapping.Properties.FilterProperties(operationType).ToList();
            var rows = new List<object[]>();

            foreach (var item in entities)
            {
                var props = item.GetType()
                    .GetProperties(BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance);
                var row = new List<object>();
                foreach (var column in tableColumns)
                {
                    var prop = props.SingleOrDefault(info => info.Name == column.PropertyName);
                    if (prop != null)
                        row.Add(prop.GetValue(item, null));
                    else if (column.IsHierarchyMapping)
                        row.Add(mapping.HierarchyMapping[item.GetType().Name]);
                    else
                        row.Add(null);
                }

                rows.Add(row.ToArray());
            }

            return new EnumerableDataReader(tableColumns.Select(propertyMapping => propertyMapping.ColumnName), rows);
        }
    }
}