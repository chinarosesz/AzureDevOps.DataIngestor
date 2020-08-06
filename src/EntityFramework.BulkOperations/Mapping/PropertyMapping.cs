namespace EntityFramework.BulkExtensions.Commons.Mapping
{
    public class PropertyMapping : IPropertyMapping
    {
        public string PropertyName { get; set; }
        public string ColumnName { get; set; }
        public bool IsPk { get; set; }
        public bool IsHierarchyMapping { get; set; }
    }
}