using EntityFramework.BulkExtensions.Commons.Context;
using EntityFramework.BulkExtensions.Commons.Mapping;
using Microsoft.Data.SqlClient;
using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace EntityFramework.BulkExtensions.Commons.Helpers
{
    internal static class SqlHelper
    {
        private const string Source = "Source";
        private const string Target = "Target";

        internal static void BulkInsertToTable<TEntity>(DbContextWrapper context, IEnumerable<TEntity> entities, string tableName, OperationType operationType) where TEntity : class
        {
            EnumerableDataReader dataReader = ToDataReader(entities, context.EntityMapping, operationType);

            IEnumerable<PropertyMapping> filteredProps = FilterProperties(context.EntityMapping.Properties, operationType);

            using SqlBulkCopy bulkcopy = new SqlBulkCopy((SqlConnection)context.Connection, SqlBulkCopyOptions.Default | SqlBulkCopyOptions.KeepIdentity, (SqlTransaction)context.Transaction);
            foreach (PropertyMapping column in filteredProps)
            {
                bulkcopy.ColumnMappings.Add(column.ColumnName, column.ColumnName);
            }
            bulkcopy.DestinationTableName = tableName;
            bulkcopy.BulkCopyTimeout = context.Connection.ConnectionTimeout;
            bulkcopy.WriteToServer(dataReader);
        }

        internal static string RandomTableName(EntityMapping mapping)
        {
            string randomTableGuid = Guid.NewGuid().ToString().Substring(0, 6);
            return $"[_{mapping.TableName}_{randomTableGuid}]";
        }

        internal static string CreateTempTableQueryString(EntityMapping mapping, string tableName, OperationType operationType)
        {
            List<PropertyMapping> columns = FilterProperties(mapping.Properties, operationType).ToList();

            List<string> paramList = columns.Select(column => $"[{column.ColumnName}]")
                .ToList();
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

        internal static string GetInsertIntoStagingTableCmd(EntityMapping mapping, string tmpOutputTableName,
            string tmpTableName, string identityColumn)
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
                    property.SetValue(items[index], result, null);

                else
                    throw new Exception();
            }

            command = GetDropTableCommand(tmpOutputTableName);
            context.ExecuteSqlCommand(command);
        }

        private static string GetDropTableCommand(string tableName)
        {
            return $"DROP TABLE {tableName};";
        }

        private static string BuildUpdateSet(EntityMapping mapping)
        {
            StringBuilder command = new StringBuilder();
            List<string> parameters = new List<string>();

            command.Append("SET ");

            foreach (PropertyMapping column in mapping.Properties.Where(propertyMapping => !propertyMapping.IsHierarchyMapping))
            {
                if (column.IsPk) continue;

                parameters.Add($"[{Target}].[{column.ColumnName}] = [{Source}].[{column.ColumnName}]");
            }

            command.Append(string.Join(", ", parameters) + " ");

            return command.ToString();
        }

        private static string PrimaryKeysComparator(EntityMapping mapping)
        {
            List<PropertyMapping> keys = mapping.Pks.ToList();
            StringBuilder command = new StringBuilder();
            PropertyMapping firstKey = keys.First();

            command.Append($"ON [{Target}].[{firstKey.ColumnName}] = [{Source}].[{firstKey.ColumnName}] ");
            keys.Remove(firstKey);

            if (keys.Any())
                foreach (PropertyMapping key in keys)
                    command.Append($"AND [{Target}].[{key.ColumnName}] = [{Source}].[{key.ColumnName}]");

            return command.ToString();
        }

        private static string BuildSelectSet(IEnumerable<string> columns, string identityColumn)
        {
            StringBuilder command = new StringBuilder();
            List<string> selectColumns = new List<string>();

            command.Append("SELECT ");

            foreach (string column in columns.ToList())
            {
                if (((identityColumn == null) || (column == identityColumn)) && (identityColumn != null)) continue;
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
                if (column != identityColumn)
                    insertColumns.Add($"[{column}]");

            command.Append(string.Join(", ", insertColumns));
            command.Append(") ");

            return command.ToString();
        }

        private static string GetOutputCreateTableCmd(string tmpTablename, string identityColumn)
        {
            return $"CREATE TABLE {tmpTablename}([{identityColumn}] int); ";
        }

        private static IEnumerable<PropertyMapping> FilterProperties(IEnumerable<PropertyMapping> propertyMappings, OperationType operationType)
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

        private static EnumerableDataReader ToDataReader<TEntity>(IEnumerable<TEntity> entities, EntityMapping mapping, OperationType operationType) where TEntity : class
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
                        row.Add(prop.GetValue(item, null));
                    else if (column.IsHierarchyMapping)
                        row.Add(mapping.HierarchyMapping[item.GetType().Name]);
                    else
                        row.Add(null);
                }

                rows.Add(row.ToArray());
            }

            return new EnumerableDataReader(tableColumns.Select(propertyMapping => propertyMapping.ColumnName), rows);
        }


    }
}