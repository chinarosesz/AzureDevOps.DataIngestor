using EntityFramework.BulkExtensions.Commons.Helpers;
using EntityFramework.BulkExtensions.Commons.Mapping;
using FastMember;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;

namespace EntityFramework.BulkOperations
{
    public static class BulkOperation
    {
        /// <summary>
        /// Bulk insert a collection of objects into the database.
        /// </summary>
        /// <returns>The number of affected rows.</returns>
        public static int BulkInsert<TEntity>(this DbContext context, IEnumerable<TEntity> entities, BulkOptions bulkOptions = BulkOptions.Default) where TEntity : class
        {
            int commitedResult = BulkOperation.CommitTransaction(context, entities, OperationType.Insert, bulkOptions);
            return commitedResult;
        }

        /// <summary>
        /// Bulk upate a collection of objects into the database
        /// </summary>
        /// <returns>The number of affected rows</returns>
        public static int BulkUpdate<TEntity>(this DbContext context, IEnumerable<TEntity> entities, BulkOptions bulkOptions = BulkOptions.Default) where TEntity : class
        {
            int commitedResult = BulkOperation.CommitTransaction(context, entities, OperationType.Update, bulkOptions);
            return commitedResult;
        }

        /// <summary>
        /// Bulk insert or update a collection of entities into database and returns the number of affected rows
        /// </summary>
        public static int BulkInsertOrUpdate<TEntity>(this DbContext context, IEnumerable<TEntity> entities, BulkOptions bulkOptions = BulkOptions.Default) where TEntity : class
        {
            int commitedResult = BulkOperation.CommitTransaction(context, entities, OperationType.InsertOrUpdate, bulkOptions);
            return commitedResult;
        }

        /// <summary>
        /// Bulk delete a collection of objects from the database.
        /// </summary>
        /// <returns>The number of affected rows.</returns>
        public static int BulkDelete<TEntity>(this DbContext context, IEnumerable<TEntity> entities, BulkOptions bulkOptions = BulkOptions.Default) where TEntity : class
        {
            int commitedResult = BulkOperation.CommitTransaction(context, entities, OperationType.Delete, bulkOptions);
            return commitedResult;
        }

        private static EntityMapping GetMapping<TEntity>(DbContext context) where TEntity : class
        {
            IEntityType entityType = context.Model.FindEntityType(typeof(TEntity));
            IEntityType baseType = entityType.BaseType ?? entityType;
            List<IEntityType> hierarchy = context.Model.GetEntityTypes()
                .Where(type => type.BaseType == null ? type == baseType : type.BaseType == baseType)
                .ToList();
            List<PropertyMapping> properties = hierarchy.GetPropertyMappings().ToList();

            EntityMapping entityMapping = new EntityMapping
            {
                TableName = entityType.GetTableName(),
                Schema = entityType.GetSchema(),
            };

            entityMapping.Properties = properties;
            return entityMapping;
        }

        private static IEnumerable<PropertyMapping> GetPropertyMappings(this IEnumerable<IEntityType> hierarchy)
        {
            return hierarchy
                .SelectMany(type => type.GetProperties().Where(property => !property.IsShadowProperty()))
                .Distinct()
                .ToList()
                .Select(property => new PropertyMapping
                {
                    PropertyName = property.Name,
                    ColumnName = property.GetColumnName(),
                    IsPk = property.IsPrimaryKey()
                });
        }

        private static string RandomTableName(EntityMapping mapping)
        {
            string randomTableGuid = Guid.NewGuid().ToString().Substring(0, 6);
            return $"[_{mapping.TableName}_{randomTableGuid}]";
        }

        private static int ExecuteSqlCommand(DbContext context, string command)
        {
            IDbCommand sqlCommand = context.Database.GetDbConnection().CreateCommand();
            sqlCommand.Transaction = context.Database.CurrentTransaction?.GetDbTransaction();
            sqlCommand.CommandTimeout = context.Database.GetCommandTimeout().Value;
            sqlCommand.CommandText = command;
            return sqlCommand.ExecuteNonQuery();
        }

        private static IDbCommand ToDbCommand(DbContext context, string command)
        {
            IDbCommand sqlCommand = context.Database.GetDbConnection().CreateCommand();
            sqlCommand.Transaction = context.Database.CurrentTransaction?.GetDbTransaction();
            sqlCommand.CommandTimeout = context.Database.GetCommandTimeout().Value;
            sqlCommand.CommandText = command;
            return sqlCommand;
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

        private static string BuildMergeCommand(EntityMapping entityMapping, string tmpTableName, OperationType operationType)
        {
            string text = string.Format("MERGE INTO {0} WITH (HOLDLOCK) AS {1} USING {2} AS {3} ", entityMapping.FullTableName, "Target", tmpTableName, "Source") + $"{BulkOperation.PrimaryKeysComparator(entityMapping)} ";
            switch (operationType)
            {
                case OperationType.Insert:
                    text += BulkOperation.BuildMergeInsertSet(entityMapping);
                    break;
                case OperationType.Update:
                    text += BulkOperation.BuildMergeUpdateSet(entityMapping);
                    break;
                case OperationType.InsertOrUpdate:
                    text += BulkOperation.BuildMergeUpdateSet(entityMapping);
                    text += BulkOperation.BuildMergeInsertSet(entityMapping);
                    break;
                case OperationType.Delete:
                    text += "WHEN MATCHED THEN DELETE";
                    break;
            }
            return text;
        }

        private static string BuildStagingTableCommand(EntityMapping mapping, string tableName, OperationType operationType, BulkOptions options)
        {
            List<PropertyMapping> source = BulkOperation.GetPropertiesByOperation(mapping, operationType).ToList();
            if (source.All((PropertyMapping s) => s.IsPk && s.IsDbGenerated) && operationType == OperationType.Update)
            {
                return null;
            }
            List<string> list = source.Select((PropertyMapping column) => string.Format("{0}.[{1}]", "Source", column.ColumnName)).ToList();
            if (BulkOperation.WillOutputGeneratedValues(mapping, options))
            {
                list.Add(string.Format("1 as [{0}]", "Bulk_Identity"));
            }
            string arg = string.Join(", ", list);
            return $"SELECT TOP 0 {arg} INTO {tableName} FROM {mapping.FullTableName} AS A " + string.Format("LEFT JOIN {0} AS {1} ON 1 = 2", mapping.FullTableName, "Source");
        }

        private static int CommitTransaction<TEntity>(DbContext dbContext, IEnumerable<TEntity> collection, OperationType operation, BulkOptions options = BulkOptions.Default) where TEntity : class
        {           
            EntityMapping entityMapping = GetMapping<TEntity>(dbContext);
            string randomTableName = BulkOperation.RandomTableName(entityMapping);
            bool outputGeneratedValues = BulkOperation.WillOutputGeneratedValues(entityMapping, options);

            // If there are no entities to insert return
            if (!collection.Any())
            {
                return collection.Count();
            }

            try
            {
                string randomTableName2 = outputGeneratedValues ? BulkOperation.RandomTableName(entityMapping) : null;
                List<PropertyMapping> entityProperties = outputGeneratedValues ? BulkOperation.GetPropertiesByOptions(entityMapping, options).ToList() : null;

                // List of SQL commands to be executed
                string stagingTableCommand = BulkOperation.BuildStagingTableCommand(entityMapping, randomTableName, operation, options);
                string mergeCommand = BulkOperation.BuildMergeCommand(entityMapping, randomTableName, operation);

                // Build staging table command and insert into table. If nothing was constructed roll back and return
                if (!string.IsNullOrEmpty(stagingTableCommand))
                {
                    BulkOperation.ExecuteSqlCommand(dbContext, stagingTableCommand);
                    BulkOperation.BulkInsertToTable(entityMapping, dbContext, collection.ToList(), randomTableName, operation, options);
                }
                else
                {
                    dbContext.Database.CurrentTransaction?.GetDbTransaction().Rollback();
                    return 0;
                }
                
                // Perform merge operation
                if (outputGeneratedValues)
                {
                    string outputTableCommand = BulkOperation.BuildOutputTableCommand(randomTableName2, entityMapping, entityProperties);
                    BulkOperation.ExecuteSqlCommand(dbContext, outputTableCommand);
                    mergeCommand += BulkOperation.BuildMergeOutputSet(randomTableName2, entityProperties);
                }
                mergeCommand += BulkOperation.GetDropTableCommand(randomTableName);
                int result = BulkOperation.ExecuteSqlCommand(dbContext, mergeCommand);

                // Load generated outputs
                if (outputGeneratedValues)
                {
                    BulkOperation.LoadFromOutputTable(dbContext, randomTableName2, entityProperties, collection.ToList());
                }

                // Commit result
                if (dbContext.Database.CurrentTransaction == null)
                {
                    dbContext.Database.GetDbConnection().BeginTransaction().Commit();
                }

                return result;
            }
            catch (Exception)
            {
                dbContext.Database.CurrentTransaction?.GetDbTransaction().Rollback();
                throw;
            }
        }

        private static void LoadFromOutputTable<TEntity>(DbContext context, string outputTableName, IEnumerable<PropertyMapping> propertyMappings, IList<TEntity> items)
        {
            IList<PropertyMapping> list = (propertyMappings as IList<PropertyMapping>) ?? propertyMappings.ToList();
            IEnumerable<string> values = list.Select((PropertyMapping property) => property.ColumnName);

            string command = string.Format("SELECT {0}, {1} FROM {2}", "Bulk_Identity", string.Join(", ", values), outputTableName);
            IDbCommand dbCommand = BulkOperation.ToDbCommand(context, command);
            IDataReader reader = dbCommand.ExecuteReader();

            using (IDataReader dataReader = reader)
            {
                while (dataReader.Read())
                {
                    object obj = items.ElementAt((int)dataReader["Bulk_Identity"]);
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
            command = BulkOperation.GetDropTableCommand(outputTableName);
            dbCommand = BulkOperation.ToDbCommand(context, command);
            dbCommand.ExecuteNonQuery();
        }

        private static string BuildMergeOutputSet(string outputTableName, IEnumerable<PropertyMapping> properties)
        {
            IList<PropertyMapping> source = (properties as IList<PropertyMapping>) ?? properties.ToList();
            string text = string.Join(", ", source.Select((PropertyMapping property) => $"INSERTED.{property.ColumnName}"));
            string text2 = string.Join(", ", source.Select((PropertyMapping property) => property.ColumnName));
            return string.Format(" OUTPUT {0}.{1}, {2} INTO {3} ({4}, {5})", "Source", "Bulk_Identity", text, outputTableName, "Bulk_Identity", text2);
        }

        private static string BuildOutputTableCommand(string tmpTablename, EntityMapping mapping, IEnumerable<PropertyMapping> propertyMappings)
        {
            return string.Format("SELECT TOP 0 1 as [{0}], {1} ", "Bulk_Identity", string.Join(", ", propertyMappings.Select((PropertyMapping property) => string.Format("{0}.[{1}]", "Source", property.ColumnName)))) + $"INTO {tmpTablename} FROM {mapping.FullTableName} AS A " + string.Format("LEFT JOIN {0} AS {1} ON 1 = 2", mapping.FullTableName, "Source");
        }

        private static void BulkInsertToTable<TEntity>(EntityMapping entityMapping, DbContext context, IList<TEntity> entities, string tableName, OperationType operationType, BulkOptions options) where TEntity : class
        {
            List<PropertyMapping> list = BulkOperation.GetPropertiesByOperation(entityMapping, operationType).ToList();
            if (BulkOperation.WillOutputGeneratedValues(entityMapping, options))
            {
                list.Add(new PropertyMapping
                {
                    ColumnName = "Bulk_Identity",
                    PropertyName = "Bulk_Identity"
                });
            }
            using (SqlBulkCopy sqlBulkCopy = new SqlBulkCopy((SqlConnection)context.Database.GetDbConnection(), SqlBulkCopyOptions.Default, (SqlTransaction)context.Database.CurrentTransaction?.GetDbTransaction()))
            {
                foreach (PropertyMapping item in list)
                {
                    sqlBulkCopy.ColumnMappings.Add(item.ColumnName, item.ColumnName);
                }
                sqlBulkCopy.BatchSize = 5000;
                sqlBulkCopy.DestinationTableName = tableName;
                sqlBulkCopy.BulkCopyTimeout = context.Database.GetCommandTimeout().Value;
                ObjectReader reader = ObjectReader.Create(entities);
                sqlBulkCopy.WriteToServer(reader);
                // sqlBulkCopy.WriteToServer((IDataReader)DataReaderHelper.ToDataReader(entities, entityMapping, list));
            }
        }

        private static IEnumerable<PropertyMapping> GetPropertiesByOptions(EntityMapping mapping, BulkOptions options)
        {
            if (options.HasFlag(BulkOptions.OutputIdentity) && options.HasFlag(BulkOptions.OutputComputed))
            {
                return mapping.Properties.Where((PropertyMapping property) => property.IsDbGenerated);
            }
            if (options.HasFlag(BulkOptions.OutputIdentity))
            {
                return mapping.Properties.Where((PropertyMapping property) => property.IsPk && property.IsDbGenerated);
            }
            if (options.HasFlag(BulkOptions.OutputComputed))
            {
                return mapping.Properties.Where((PropertyMapping property) => !property.IsPk && property.IsDbGenerated);
            }
            return mapping.Properties;
        }

        private static string GetDropTableCommand(string tableName)
        {
            return $"; DROP TABLE {tableName};";
        }

        private static IEnumerable<PropertyMapping> GetPropertiesByOperation(EntityMapping mapping, OperationType operationType)
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
    }
}