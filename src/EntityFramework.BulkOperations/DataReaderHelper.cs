using EntityFramework.BulkExtensions.Commons.Mapping;
using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace EntityFramework.BulkOperations
{
    internal class DataReaderHelper
    {
		internal static EnumerableDataReader ToDataReader<TEntity>(IList<TEntity> entities, EntityMapping mapping, IEnumerable<PropertyMapping> tableColumns) where TEntity : class
		{
			List<object[]> list = new List<object[]>();
			IList<PropertyMapping> list2 = (tableColumns as IList<PropertyMapping>) ?? tableColumns.ToList();
			for (int i = 0; i < entities.Count; i++)
			{
				TEntity val = entities[i];
				EntryWrapper entryWrapper = val as EntryWrapper;
				object obj = (entryWrapper != null) ? entryWrapper.Entity : val;
				Type type = obj.GetType();
				List<PropertyInfo> source = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy).ToList();
				List<object> list3 = new List<object>();
				foreach (PropertyMapping propertyMapping2 in list2)
				{
					PropertyInfo propertyInfo = source.SingleOrDefault((PropertyInfo info) => info.Name == propertyMapping2.PropertyName);
					if (propertyInfo != null && !propertyMapping2.IsFk)
					{
						list3.Add(propertyInfo.GetValue(obj, null) ?? DBNull.Value);
					}
					else if (propertyMapping2.IsFk && entryWrapper != null)
					{
						list3.Add(GetForeingKeyValue(entryWrapper, propertyMapping2));
					}
					else if (propertyMapping2.IsFk && propertyInfo != null)
					{
						list3.Add(propertyInfo.GetValue(obj, null) ?? DBNull.Value);
					}
					else if (propertyMapping2.IsHierarchyMapping)
					{
						list3.Add(mapping.HierarchyMapping[type.Name]);
					}
					else if (propertyMapping2.PropertyName.Equals("Bulk_Identity"))
					{
						list3.Add(i);
					}
					else
					{
						list3.Add(DBNull.Value);
					}
				}
				list.Add(list3.ToArray());
			}
			return new EnumerableDataReader(list2.Select((PropertyMapping propertyMapping) => propertyMapping.ColumnName), list);
		}

		private static object GetForeingKeyValue(EntryWrapper wrapper, PropertyMapping propertyMapping)
		{
			if (wrapper?.ForeignKeys == null)
			{
				return DBNull.Value;
			}
			if (wrapper.ForeignKeys.TryGetValue(propertyMapping.ForeignKeyName, out object value))
			{
				return value;
			}
			return DBNull.Value;
		}
	}
}
