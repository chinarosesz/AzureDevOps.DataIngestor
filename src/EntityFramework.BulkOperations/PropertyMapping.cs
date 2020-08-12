namespace EntityFramework.BulkExtensions.Commons.Mapping
{
    public class PropertyMapping
    {
        public string PropertyName { get; set; }
        public string ColumnName { get; set; }
        public bool IsPk { get; set; }
        public bool IsFk { get; set; }
        public bool IsHierarchyMapping { get; set; }
        public bool IsDbGenerated { get; }
        public string ForeignKeyName { get; set; }
    } 
}