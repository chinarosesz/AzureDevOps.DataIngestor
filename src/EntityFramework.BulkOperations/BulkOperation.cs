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
using System.Runtime.CompilerServices;
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

        private static string RandomTableName(EntityMapping mapping)
        {
            string randomTableGuid = Guid.NewGuid().ToString().Substring(0, 6);
            return $"[_{mapping.TableName}_{randomTableGuid}]";
        }

        private static int ExecuteSqlCommandNonQuery(DbContext context, string command)
        {
            IDbCommand sqlCommand = context.Database.GetDbConnection().CreateCommand();
            sqlCommand.Transaction = context.Database.CurrentTransaction?.GetDbTransaction();
            sqlCommand.CommandTimeout = context.Database.GetCommandTimeout().Value;
            sqlCommand.CommandText = command;
            return sqlCommand.ExecuteNonQuery();
        }

        private static string PrimaryKeysComparator(EntityMapping mapping)
        {
            List<IProperty> primaryKeys = mapping.PrimaryKeyProperties.ToList();
            StringBuilder stringBuilder = new StringBuilder();
            IProperty firstPrimaryKey = primaryKeys.First();

            stringBuilder.Append(string.Format("ON [{0}].[{1}] = [{2}].[{3}] ", "Target", firstPrimaryKey.GetColumnName(), "Source", firstPrimaryKey.GetColumnName()));
            
            primaryKeys.Remove(firstPrimaryKey);

            if (primaryKeys.Any())
            {
                foreach (IProperty item in primaryKeys)
                {
                    stringBuilder.Append(string.Format("AND [{0}].[{1}] = [{2}].[{3}]", "Target", item.GetColumnName(), "Source", item.GetColumnName()));
                }
            }
            return stringBuilder.ToString();
        }

        private static string BuildMergeInsertSet(EntityMapping mapping)
        {
            StringBuilder stringBuilder = new StringBuilder();
            List<string> list = new List<string>();
            List<string> list2 = new List<string>();
            //List<IProperty> list3 = mapping.EntityProperties.Where((IProperty entityProp) => entityProp.ValueGenerated != ValueGenerated.Never).ToList();
            List<IProperty> list3 = mapping.EntityProperties.ToList();
            stringBuilder.Append(" WHEN NOT MATCHED BY TARGET THEN INSERT ");
            if (list3.Any())
            {
                foreach (IProperty item in list3)
                {
                    list.Add($"[{item.GetColumnName()}]");
                    list2.Add(string.Format("[{0}].[{1}]", "Source", item.GetColumnName()));
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
            List<IProperty> list2 = (from entityProp in mapping.EntityProperties
                                           where !entityProp.IsPrimaryKey()
                                           where entityProp.ValueGenerated != ValueGenerated.Never
                                           select entityProp).ToList();
            if (list2.Any())
            {
                stringBuilder.Append("WHEN MATCHED THEN UPDATE SET ");
                foreach (IProperty item in list2)
                {
                    list.Add(string.Format("[{0}].[{1}] = [{2}].[{3}]", "Target", item.GetColumnName(), "Source", item.GetColumnName()));
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
            List<IProperty> source = BulkOperation.GetPropertiesByOperation(mapping, operationType).ToList();

            if (source.All((IProperty s) => s.IsPrimaryKey() && s.ValueGenerated == ValueGenerated.OnAddOrUpdate) && operationType == OperationType.Update)
            {
                return null;
            }

            List<string> list = source.Select((IProperty column) => string.Format("{0}.[{1}]", "Source", column.GetColumnName())).ToList();
            if (BulkOperation.WillOutputGeneratedValues(mapping, options))
            {
                list.Add(string.Format("1 as [{0}]", "Bulk_Identity"));
            }
            string arg = string.Join(", ", list);
            return $"SELECT TOP 0 {arg} INTO {tableName} FROM {mapping.FullTableName} AS A " + string.Format("LEFT JOIN {0} AS {1} ON 1 = 2", mapping.FullTableName, "Source");
        }

        private static int CommitTransaction<TEntity>(DbContext context, IEnumerable<TEntity> collection, OperationType operation, BulkOptions options = BulkOptions.Default) where TEntity : class
        {           
            IEntityType entityType = context.Model.FindEntityType(typeof(TEntity));
            EntityMapping entityMapping = new EntityMapping(entityType);

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
                IEnumerable<IProperty> entityProps = outputGeneratedValues ? BulkOperation.GetPropertiesByOptions(entityMapping, options) : null;

                // List of SQL commands to be executed
                string stagingTableCommand = BulkOperation.BuildStagingTableCommand(entityMapping, randomTableName, operation, options);
                string mergeCommand = BulkOperation.BuildMergeCommand(entityMapping, randomTableName, operation);

                // Build staging table command and insert into table. If nothing was constructed roll back and return
                if (!string.IsNullOrEmpty(stagingTableCommand))
                {
                    BulkOperation.ExecuteSqlCommandNonQuery(context, stagingTableCommand);
                    BulkOperation.BulkInsertToTable(entityMapping, context, collection.ToList(), randomTableName, operation, options);
                }
                else
                {
                    context.Database.CurrentTransaction?.GetDbTransaction().Rollback();
                    return 0;
                }
                
                // Perform merge operation
                if (outputGeneratedValues)
                {
                    string outputTableCommand = BulkOperation.BuildOutputTableCommand(randomTableName2, entityMapping, entityProps);
                    BulkOperation.ExecuteSqlCommandNonQuery(context, outputTableCommand);
                    mergeCommand += BulkOperation.BuildMergeOutputSet(randomTableName2, entityProps);
                }
                mergeCommand += BulkOperation.GetDropTableCommand(randomTableName);
                int result = BulkOperation.ExecuteSqlCommandNonQuery(context, mergeCommand);

                // Load generated outputs
                if (outputGeneratedValues)
                {
                    BulkOperation.LoadFromOutputTable(context, randomTableName2, entityProps, collection.ToList());
                }

                // Commit result
                if (context.Database.CurrentTransaction == null)
                {
                    context.Database.GetDbConnection().BeginTransaction().Commit();
                }

                return result;
            }
            catch (Exception)
            {
                context.Database.CurrentTransaction?.GetDbTransaction().Rollback();
                throw;
            }
        }

        private static void LoadFromOutputTable<TEntity>(DbContext context, string outputTableName, IEnumerable<IProperty> entityProps, IList<TEntity> items)
        {
            IList<IProperty> list = (entityProps as IList<IProperty>) ?? entityProps.ToList();
            IEnumerable<string> values = list.Select((IProperty property) => property.GetColumnName());

            IDbCommand sqlCommand = context.Database.GetDbConnection().CreateCommand();
            sqlCommand.Transaction = context.Database.CurrentTransaction?.GetDbTransaction();
            sqlCommand.CommandTimeout = context.Database.GetCommandTimeout().Value;
            sqlCommand.CommandText = string.Format("SELECT {0}, {1} FROM {2}", "Bulk_Identity", string.Join(", ", values), outputTableName);
            IDataReader reader = sqlCommand.ExecuteReader();

            using (IDataReader dataReader = reader)
            {
                while (dataReader.Read())
                {
                    object obj = items.ElementAt((int)dataReader["Bulk_Identity"]);
                    foreach (IProperty item in list)
                    {
                        PropertyInfo property2 = obj.GetType().GetProperty(item.Name);
                        if (property2 != null && property2.CanWrite)
                        {
                            property2.SetValue(obj, dataReader[item.GetColumnName()], null);
                            continue;
                        }
                        throw new Exception("Field not existent");
                    }
                }
            }
            string command = BulkOperation.GetDropTableCommand(outputTableName);
            BulkOperation.ExecuteSqlCommandNonQuery(context, command);
        }

        private static string BuildMergeOutputSet(string outputTableName, IEnumerable<IProperty> properties)
        {
            IList<IProperty> source = (properties as IList<IProperty>) ?? properties.ToList();
            string text = string.Join(", ", source.Select((IProperty property) => $"INSERTED.{property.GetColumnName()}"));
            string text2 = string.Join(", ", source.Select((IProperty property) => property.GetColumnName()));
            return string.Format(" OUTPUT {0}.{1}, {2} INTO {3} ({4}, {5})", "Source", "Bulk_Identity", text, outputTableName, "Bulk_Identity", text2);
        }

        private static string BuildOutputTableCommand(string tmpTablename, EntityMapping mapping, IEnumerable<IProperty> propertyMappings)
        {
            return string.Format("SELECT TOP 0 1 as [{0}], {1} ", "Bulk_Identity", string.Join(", ", propertyMappings.Select((IProperty property) => string.Format("{0}.[{1}]", "Source", property.GetColumnName())))) + $"INTO {tmpTablename} FROM {mapping.FullTableName} AS A " + string.Format("LEFT JOIN {0} AS {1} ON 1 = 2", mapping.FullTableName, "Source");
        }

        private static void BulkInsertToTable<TEntity>(EntityMapping entityMapping, DbContext context, IList<TEntity> entities, string tableName, OperationType operationType, BulkOptions options) where TEntity : class
        {
            List<IProperty> props = BulkOperation.GetPropertiesByOperation(entityMapping, operationType).ToList();

            using (SqlBulkCopy sqlBulkCopy = new SqlBulkCopy((SqlConnection)context.Database.GetDbConnection(), SqlBulkCopyOptions.Default, (SqlTransaction)context.Database.CurrentTransaction?.GetDbTransaction()))
            {
                foreach (IProperty prop in props)
                {
                    sqlBulkCopy.ColumnMappings.Add(prop.GetColumnName(), prop.GetColumnName());
                }

                if (BulkOperation.WillOutputGeneratedValues(entityMapping, options))
                {
                    sqlBulkCopy.ColumnMappings.Add("Bulk_Identity", "Bulk_Identity");
                }

                sqlBulkCopy.BatchSize = 5000;
                sqlBulkCopy.DestinationTableName = tableName;
                sqlBulkCopy.BulkCopyTimeout = context.Database.GetCommandTimeout().Value;
                ObjectReader reader = ObjectReader.Create(entities);
                sqlBulkCopy.WriteToServer(reader);
            }
        }

        private static IEnumerable<IProperty> GetPropertiesByOptions(EntityMapping mapping, BulkOptions options)
        {
            IEnumerable<IProperty> outputProps = null;

            if (options.HasFlag(BulkOptions.OutputIdentity) && options.HasFlag(BulkOptions.OutputComputed))
            {
                outputProps = mapping.EntityProperties.Where(v => v.IsPrimaryKey() && (v.ValueGenerated == ValueGenerated.OnAddOrUpdate || v.ValueGenerated == ValueGenerated.OnAddOrUpdate));
            }
            else if (options.HasFlag(BulkOptions.OutputIdentity))
            {
                outputProps = mapping.EntityProperties.Where(v => v.IsPrimaryKey() && v.ValueGenerated == ValueGenerated.OnAdd);
            }
            else if (options.HasFlag(BulkOptions.OutputComputed))
            {
                outputProps = mapping.EntityProperties.Where(v => v.IsPrimaryKey() && v.ValueGenerated == ValueGenerated.OnAddOrUpdate);
            }

            return outputProps;
        }

        private static string GetDropTableCommand(string tableName)
        {
            return $"; DROP TABLE {tableName};";
        }

        private static IEnumerable<IProperty> GetPropertiesByOperation(EntityMapping mapping, OperationType operationType)
        {
            if (operationType == OperationType.Delete)
            {
                return mapping.EntityProperties.Where(v => v.IsPrimaryKey());
            }
            else
            {
                return mapping.EntityProperties;
            }
        }

        private static bool WillOutputGeneratedValues(EntityMapping mapping, BulkOptions options)
        {
            IEnumerable<IProperty> computedColumns = mapping.EntityProperties.Where(v => v.IsPrimaryKey() && v.ValueGenerated == ValueGenerated.OnAddOrUpdate);
            IEnumerable<IProperty> generatedKeys = mapping.EntityProperties.Where(v => v.IsPrimaryKey() && v.ValueGenerated == ValueGenerated.OnAdd);
            return ((options.HasFlag(BulkOptions.OutputIdentity) && generatedKeys.Any()) || (options.HasFlag(BulkOptions.OutputComputed) && computedColumns.Any()));
        }
    }
}