using EntityFramework.BulkExtensions.Commons.Context;
using EntityFramework.BulkExtensions.Commons.Mapping;
using EntityFramework.BulkOperations;
using Microsoft.Data.SqlClient;
using Shared.Helpers;
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
        private const string Source = "Source";
        private const string Target = "Target";

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

        internal static string CreateTempTableQueryString(EntityMapping mapping, string tableName, OperationType operationType)
        {
            List<PropertyMapping> columns = FilterProperties(mapping.Properties, operationType).ToList();
            List<string> paramList = columns.Select(column => $"[{column.ColumnName}]").ToList();
            string paramListConcatenated = string.Join(", ", paramList);

            return $"SELECT {paramListConcatenated} INTO {tableName} FROM {mapping.TableName} WHERE 1 = 2";
        }

        internal static string BuildDeleteCommand(DbContextWrapper context, string tmpTableName)
        {
            return $"MERGE INTO {context.EntityMapping.FullTableName} WITH (HOLDLOCK) AS Target USING {tmpTableName} AS Source " +
                   $"{SqlHelper.PrimaryKeysComparator(context.EntityMapping)} WHEN MATCHED THEN DELETE;" +
                   GetDropTableCommand(tmpTableName);
        }

        internal static string BuildMergeCommand(DbContextWrapper context, string tmpTableName)
        {
            return $"MERGE INTO {context.EntityMapping.FullTableName} WITH (HOLDLOCK) AS Target USING {tmpTableName} AS Source " +
                   $"{SqlHelper.PrimaryKeysComparator(context.EntityMapping)} WHEN MATCHED THEN UPDATE {SqlHelper.BuildUpdateSet(context.EntityMapping)}; " +
                   GetDropTableCommand(tmpTableName);
        }

        internal static bool WillOutputGeneratedValues(EntityMapping mapping, BulkOptions options)
        {
            return (options.HasFlag(BulkOptions.OutputIdentity) && mapping.HasGeneratedKeys) || (options.HasFlag(BulkOptions.OutputComputed) && mapping.HasComputedColumns);
        }

        internal static string BuildInsertOrUpdateCommand(DbContextWrapper context, string tempTableName)
        {
            List<string> columns = context.EntityMapping.Properties.Select(propertyMapping => propertyMapping.ColumnName).ToList();
            List<bool> identityColumns = context.EntityMapping.Pks.Select(v => v.IsPk).ToList();
            var identityColumn = context.EntityMapping.Pks.First().ColumnName;
            string insertCommand = SqlHelper.BuildInsertIntoSet(columns, identityColumn, tempTableName);

            return $"MERGE INTO {context.EntityMapping.FullTableName} WITH (HOLDLOCK) AS Target USING {tempTableName} AS Source " +
                   $"{SqlHelper.PrimaryKeysComparator(context.EntityMapping)} WHEN MATCHED THEN UPDATE {SqlHelper.BuildUpdateSet(context.EntityMapping)} " +
                   $"{SqlHelper.PrimaryKeysComparator(context.EntityMapping)} WHEN NOT MATCHED THEN {insertCommand}; " +
                   GetDropTableCommand(tempTableName);
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

        internal static string GetInsertIntoStagingTableCmd(EntityMapping mapping, string tmpOutputTableName, string tmpTableName, string identityColumn)
        {
            List<string> columns = mapping.Properties.Select(propertyMapping => propertyMapping.ColumnName).ToList();

            string comm = GetOutputCreateTableCmd(tmpOutputTableName, identityColumn)
                       + BuildInsertIntoSet(columns, identityColumn, mapping.FullTableName)
                       + $"OUTPUT INSERTED.{identityColumn} INTO "
                       + tmpOutputTableName + $"([{identityColumn}]) "
                       + BuildSelectSet(columns, identityColumn)
                       + $" FROM {tmpTableName} AS Source; "
                       + GetDropTableCommand(tmpTableName);

            return comm;
        }

        internal static void LoadFromTmpOutputTable<TEntity>(DbContextWrapper context, string tmpOutputTableName, PropertyMapping propertyMapping, IList<TEntity> items)
        {
            string command = $"SELECT {propertyMapping.ColumnName} FROM {tmpOutputTableName} ORDER BY {propertyMapping.ColumnName};";
            List<int> identities = context.SqlQuery<int>(command).ToList();

            foreach (int result in identities)
            {
                int index = identities.IndexOf(result);
                PropertyInfo property = items[index].GetType().GetProperty(propertyMapping.PropertyName);

                if (property != null && property.CanWrite)
                {
                    property.SetValue(items[index], result, null);
                }
                else
                {
                    throw new Exception();
                }
            }

            command = GetDropTableCommand(tmpOutputTableName);
            context.ExecuteSqlCommand(command);
        }

        private static string BuildUpdateSet(EntityMapping mapping)
        {
            StringBuilder command = new StringBuilder();
            List<string> parameters = new List<string>();

            command.Append("SET ");

            foreach (PropertyMapping column in mapping.Properties.Where(propertyMapping => !propertyMapping.IsHierarchyMapping))
            {
                if (column.IsPk)
                {
                    continue;
                }

                parameters.Add($"[{Target}].[{column.ColumnName}] = [{Source}].[{column.ColumnName}]");
            }

            command.Append(string.Join(", ", parameters) + " ");

            return command.ToString();
        }

        private static string BuildSelectSet(IEnumerable<string> columns, string identityColumn)
        {
            StringBuilder command = new StringBuilder();
            List<string> selectColumns = new List<string>();

            command.Append("SELECT ");

            foreach (string column in columns.ToList())
            {
                if (((identityColumn == null) || (column == identityColumn)) && (identityColumn != null))
                {
                    continue;
                }

                selectColumns.Add($"[{Source}].[{column}]");
            }

            command.Append(string.Join(", ", selectColumns));

            return command.ToString();
        }

        private static string BuildInsertIntoSet(IEnumerable<string> columns, string identityColumn, string tableName)
        {
            StringBuilder command = new StringBuilder();
            List<string> insertColumns = new List<string>();

            command.Append("INSERT INTO ");
            command.Append(tableName);
            command.Append(" (");

            foreach (string column in columns)
            {
                if (column != identityColumn)
                {
                    insertColumns.Add($"[{column}]");
                }
            }

            command.Append(string.Join(", ", insertColumns));
            command.Append(")");

            return command.ToString();
        }

        private static string GetOutputCreateTableCmd(string tmpTablename, string identityColumn)
        {
            return $"CREATE TABLE {tmpTablename}([{identityColumn}] int); ";
        }

        internal static IEnumerable<PropertyMapping> FilterProperties(IEnumerable<PropertyMapping> propertyMappings, OperationType operationType)
        {
            switch (operationType)
            {
                case OperationType.Delete:
                    return propertyMappings.Where(propertyMapping => propertyMapping.IsPk).ToList();
                case OperationType.Update:
                    return propertyMappings.Where(propertyMapping => !propertyMapping.IsHierarchyMapping).ToList();
                default:
                    return propertyMappings;
            }
        }

        internal static EnumerableDataReader ToDataReader<TEntity>(IEnumerable<TEntity> entities, EntityMapping mapping) where TEntity : class
        {
            //List<IPropertyMapping> tableColumns = mapping.Properties.FilterProperties(operationType).ToList();
            List<PropertyMapping> tableColumns = mapping.Properties.ToList();
            List<object[]> rows = new List<object[]>();

            foreach (TEntity item in entities)
            {
                PropertyInfo[] props = item.GetType().GetProperties(BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance);
                List<object> row = new List<object>();
                foreach (PropertyMapping column in tableColumns)
                {
                    PropertyInfo prop = props.SingleOrDefault(info => info.Name == column.PropertyName);
                    if (prop != null)
                    {
                        row.Add(prop.GetValue(item, null));
                    }
                    else if (column.IsHierarchyMapping)
                    {
                        row.Add(mapping.HierarchyMapping[item.GetType().Name]);
                    }
                    else
                    {
                        row.Add(null);
                    }
                }

                rows.Add(row.ToArray());
            }

            return new EnumerableDataReader(tableColumns.Select(propertyMapping => propertyMapping.ColumnName), rows);
        }


    }
}