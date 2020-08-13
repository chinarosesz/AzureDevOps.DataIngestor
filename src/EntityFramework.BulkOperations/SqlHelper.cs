using EntityFramework.BulkExtensions.Commons.Context;
using EntityFramework.BulkExtensions.Commons.Mapping;
using EntityFramework.BulkOperations;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;

namespace EntityFramework.BulkExtensions.Commons.Helpers
{
    internal static class SqlHelper
    {


        internal static string GetDropTableCommand(string tableName)
        {
            return $"; DROP TABLE {tableName};";
        }





 



        internal static IEnumerable<PropertyMapping> GetPropertiesByOperation(EntityMapping mapping, OperationType operationType)
        {
            switch (operationType)
            {
                case OperationType.Delete:
                    return mapping.Properties.Where((PropertyMapping propertyMapping) => propertyMapping.IsPk);
                case OperationType.Update:
                    return from propertyMapping in mapping.Properties
                           where !propertyMapping.IsHierarchyMapping
                           where propertyMapping.IsPk || !propertyMapping.IsDbGenerated
                           select propertyMapping;
                default:
                    return mapping.Properties.Where((PropertyMapping propertyMapping) => propertyMapping.IsPk || !propertyMapping.IsDbGenerated);
            }
        }

        internal static bool WillOutputGeneratedValues(EntityMapping mapping, BulkOptions options)
        {
            return (options.HasFlag(BulkOptions.OutputIdentity) && mapping.HasGeneratedKeys) || (options.HasFlag(BulkOptions.OutputComputed) && mapping.HasComputedColumns);
        }
    }
}