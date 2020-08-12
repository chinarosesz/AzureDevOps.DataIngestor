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
        internal static string RandomTableName(EntityMapping mapping)
        {
            string randomTableGuid = Guid.NewGuid().ToString().Substring(0, 6);
            return $"[_{mapping.TableName}_{randomTableGuid}]";
        }

        internal static string GetDropTableCommand(string tableName)
        {
            return $"; DROP TABLE {tableName};";
        }

        internal static void LoadFromOutputTable<TEntity>(DbContextWrapper context, string outputTableName, IEnumerable<PropertyMapping> propertyMappings, IList<TEntity> items)
        {
            IList<PropertyMapping> list = (propertyMappings as IList<PropertyMapping>) ?? propertyMappings.ToList();
            IEnumerable<string> values = list.Select((PropertyMapping property) => property.ColumnName);
            string command = string.Format("SELECT {0}, {1} FROM {2}", "Bulk_Identity", string.Join(", ", values), outputTableName);
            using (IDataReader dataReader = context.SqlQuery(command))
            {
                while (dataReader.Read())
                {
                    object obj = items.ElementAt((int)dataReader["Bulk_Identity"]);
                    EntryWrapper entryWrapper;
                    if ((entryWrapper = (obj as EntryWrapper)) != null)
                    {
                        obj = entryWrapper.Entity;
                    }
                    foreach (PropertyMapping item in list)
                    {
                        PropertyInfo property2 = obj.GetType().GetProperty(item.PropertyName);
                        if (property2 != null && property2.CanWrite)
                        {
                            property2.SetValue(obj, dataReader[item.ColumnName], null);
                            continue;
                        }
                        throw new Exception("Field not existent");
                    }
                }
            }
            command = SqlHelper.GetDropTableCommand(outputTableName);
            context.ExecuteSqlCommand(command);
        }

        internal static string BuildMergeOutputSet(string outputTableName, IEnumerable<PropertyMapping> properties)
        {
            IList<PropertyMapping> source = (properties as IList<PropertyMapping>) ?? properties.ToList();
            string text = string.Join(", ", source.Select((PropertyMapping property) => $"INSERTED.{property.ColumnName}"));
            string text2 = string.Join(", ", source.Select((PropertyMapping property) => property.ColumnName));
            return string.Format(" OUTPUT {0}.{1}, {2} INTO {3} ({4}, {5})", "Source", "Bulk_Identity", text, outputTableName, "Bulk_Identity", text2);
        }

        internal static string BuildOutputTableCommand(string tmpTablename, EntityMapping mapping, IEnumerable<PropertyMapping> propertyMappings)
        {
            return string.Format("SELECT TOP 0 1 as [{0}], {1} ", "Bulk_Identity", string.Join(", ", propertyMappings.Select((PropertyMapping property) => string.Format("{0}.[{1}]", "Source", property.ColumnName)))) + $"INTO {tmpTablename} FROM {mapping.FullTableName} AS A " + string.Format("LEFT JOIN {0} AS {1} ON 1 = 2", mapping.FullTableName, "Source");
        }

        internal static string BuildStagingTableCommand(EntityMapping mapping, string tableName, OperationType operationType, BulkOptions options)
        {
            List<PropertyMapping> source = SqlHelper.GetPropertiesByOperation(mapping, operationType).ToList();
            if (source.All((PropertyMapping s) => s.IsPk && s.IsDbGenerated) && operationType == OperationType.Update)
            {
                return null;
            }
            List<string> list = source.Select((PropertyMapping column) => string.Format("{0}.[{1}]", "Source", column.ColumnName)).ToList();
            if (SqlHelper.WillOutputGeneratedValues(mapping, options))
            {
                list.Add(string.Format("1 as [{0}]", "Bulk_Identity"));
            }
            string arg = string.Join(", ", list);
            return $"SELECT TOP 0 {arg} INTO {tableName} FROM {mapping.FullTableName} AS A " + string.Format("LEFT JOIN {0} AS {1} ON 1 = 2", mapping.FullTableName, "Source");
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

        internal static string BuildMergeCommand(DbContextWrapper context, string tmpTableName, OperationType operationType)
        {
            string text = string.Format("MERGE INTO {0} WITH (HOLDLOCK) AS {1} USING {2} AS {3} ", context.EntityMapping.FullTableName, "Target", tmpTableName, "Source") + $"{SqlHelper.PrimaryKeysComparator(context.EntityMapping)} ";
            switch (operationType)
            {
                case OperationType.Insert:
                    text += SqlHelper.BuildMergeInsertSet(context.EntityMapping);
                    break;
                case OperationType.Update:
                    text += SqlHelper.BuildMergeUpdateSet(context.EntityMapping);
                    break;
                case OperationType.InsertOrUpdate:
                    text += SqlHelper.BuildMergeUpdateSet(context.EntityMapping);
                    text += SqlHelper.BuildMergeInsertSet(context.EntityMapping);
                    break;
                case OperationType.Delete:
                    text += "WHEN MATCHED THEN DELETE";
                    break;
            }
            return text;
        }

        private static string PrimaryKeysComparator(EntityMapping mapping)
        {
            List<PropertyMapping> list = mapping.Pks.ToList();
            StringBuilder stringBuilder = new StringBuilder();
            PropertyMapping propertyMapping = list.First();
            stringBuilder.Append(string.Format("ON [{0}].[{1}] = [{2}].[{3}] ", "Target", propertyMapping.ColumnName, "Source", propertyMapping.ColumnName));
            list.Remove(propertyMapping);
            if (list.Any())
            {
                foreach (PropertyMapping item in list)
                {
                    stringBuilder.Append(string.Format("AND [{0}].[{1}] = [{2}].[{3}]", "Target", item.ColumnName, "Source", item.ColumnName));
                }
            }
            return stringBuilder.ToString();
        }

        private static string BuildMergeUpdateSet(EntityMapping mapping)
        {
            StringBuilder stringBuilder = new StringBuilder();
            List<string> list = new List<string>();
            List<PropertyMapping> list2 = (from propertyMapping in mapping.Properties
                                            where !propertyMapping.IsPk
                                            where !propertyMapping.IsDbGenerated
                                            where !propertyMapping.IsHierarchyMapping
                                            select propertyMapping).ToList();
            if (list2.Any())
            {
                stringBuilder.Append("WHEN MATCHED THEN UPDATE SET ");
                foreach (PropertyMapping item in list2)
                {
                    list.Add(string.Format("[{0}].[{1}] = [{2}].[{3}]", "Target", item.ColumnName, "Source", item.ColumnName));
                }
                stringBuilder.Append(string.Join(", ", list) + " ");
            }
            return stringBuilder.ToString();
        }

        private static string BuildMergeInsertSet(EntityMapping mapping)
        {
            StringBuilder stringBuilder = new StringBuilder();
            List<string> list = new List<string>();
            List<string> list2 = new List<string>();
            List<PropertyMapping> list3 = mapping.Properties.Where((PropertyMapping propertyMapping) => !propertyMapping.IsDbGenerated).ToList();
            stringBuilder.Append(" WHEN NOT MATCHED BY TARGET THEN INSERT ");
            if (list3.Any())
            {
                foreach (PropertyMapping item in list3)
                {
                    list.Add($"[{item.ColumnName}]");
                    list2.Add(string.Format("[{0}].[{1}]", "Source", item.ColumnName));
                }
                stringBuilder.Append(string.Format("({0}) VALUES ({1})", string.Join(", ", list), string.Join(", ", list2)));
            }
            else
            {
                stringBuilder.Append("DEFAULT VALUES");
            }
            return stringBuilder.ToString();
        }
    }
}