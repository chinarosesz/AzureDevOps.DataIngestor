using System.Collections.Generic;
using System.Linq;

namespace EntityFramework.BulkExtensions.Commons.Mapping
{
    public class EntityMapping : IEntityMapping
    {
        public string TableName { get; set; }
        public string Schema { get; set; }
        public IEnumerable<IPropertyMapping> Properties { get; set; }

        public IEnumerable<IPropertyMapping> Pks
        {
            get { return Properties.Where(propertyMapping => propertyMapping.IsPk); }
        }

        public string FullTableName => Schema != null ? $"[{Schema}].[{TableName}]" : $"[{TableName}]";

        public Dictionary<string, string> HierarchyMapping { get; set; }
    }
}