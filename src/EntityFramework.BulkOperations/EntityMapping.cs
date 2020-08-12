using System.Collections.Generic;
using System.Linq;

namespace EntityFramework.BulkExtensions.Commons.Mapping
{
    public class EntityMapping
    {
        public string TableName { get; set; }
        
        public string Schema { get; set; }
        
        public IEnumerable<PropertyMapping> Properties { get; set; }
        
        public IEnumerable<PropertyMapping> Pks => Properties.Where((PropertyMapping propertyMapping) => propertyMapping.IsPk);

        public string FullTableName => string.IsNullOrEmpty(Schema?.Trim()) ? $"[{TableName}]" : $"[{Schema}].[{TableName}]";

        public bool HasGeneratedKeys => Properties.Any((PropertyMapping property) => property.IsPk && property.IsDbGenerated);

        public bool HasComputedColumns => Properties.Any((PropertyMapping property) => !property.IsPk && property.IsDbGenerated);

        public Dictionary<string, string> HierarchyMapping { get; set; }
    }
}