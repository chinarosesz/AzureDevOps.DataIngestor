namespace EntityFramework.BulkExtensions.Commons.Mapping
{
    public interface IPropertyMapping
    {
        string PropertyName { get; set; }
        string ColumnName { get; set; }
        bool IsPk { get; set; }
        bool IsHierarchyMapping { get; set; }
    }
}