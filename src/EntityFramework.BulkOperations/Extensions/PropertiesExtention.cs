using System.Collections.Generic;
using System.Linq;
using EntityFramework.BulkExtensions.Commons.Helpers;
using EntityFramework.BulkExtensions.Commons.Mapping;


namespace EntityFramework.BulkExtensions.Commons.Extensions
{
    internal static class PropertiesExtention
    {
        internal static IEnumerable<IPropertyMapping> FilterProperties(this IEnumerable<IPropertyMapping> propertyMappings, OperationType operationType)
        {
            switch (operationType)
            {
                case OperationType.Insert:
                    return propertyMappings.Where(propertyMapping => !propertyMapping.IsPk).ToList();
                case OperationType.Delete:
                    return propertyMappings.Where(propertyMapping => propertyMapping.IsPk).ToList();
                case OperationType.Update:
                    return propertyMappings.Where(propertyMapping => !propertyMapping.IsHierarchyMapping).ToList();
                default:
                    return propertyMappings;
            }
        }
    }
}