using FastMember;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace EntityFramework.BulkOperations
{
    public static class BulkOperationHelper
    {
        internal static int CommitTransaction<TEntity>(DbContext context, IEnumerable<TEntity> collection, OperationType operation) where TEntity : class
        {
            IEntityType entityType = context.Model.FindEntityType(typeof(TEntity));
            EntityMapping entityMapping = new EntityMapping(entityType);

            // No entities to commit
            if (!collection.Any())
            {
                return collection.Count();
            }

            try
            {
                // Sample random table looks like this [_VssBuildDefinition_02da3b]
                string randomTableName = $"[_{entityMapping.TableName}_{Guid.NewGuid().ToString().Substring(0, 6)}]";

                // Commit staging table command
                string stagingTableCommand = BulkOperationHelper.BuildStagingTableCommand(entityMapping, randomTableName, operation);
                if (!string.IsNullOrEmpty(stagingTableCommand))
                {
                    BulkOperationHelper.ExecuteSqlCommandNonQuery(context, stagingTableCommand);
                    BulkOperationHelper.BulkInsertToTable(entityMapping, context, collection.ToList(), randomTableName, operation);
                }
                else
                {
                    context.Database.CurrentTransaction?.GetDbTransaction().Rollback();
                    return 0;
                }

                // Commit merge command
                string mergeCommand = BulkOperationHelper.BuildMergeCommand(entityMapping, randomTableName, operation);
                int result = BulkOperationHelper.ExecuteSqlCommandNonQuery(context, mergeCommand);
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

        private static string BuildMergeCommand(EntityMapping entity, string tempTableName, OperationType operationType)
        {
            // Merge command looks like:
            // MERGE INTO [VssBuildDefinition] WITH (HOLDLOCK) AS Target USING [_VssBuildDefinition_02da3b] AS Source ON [Target].[Id] = [Source].[Id] AND [Target].[ProjectId] = [Source].[ProjectId] 
            StringBuilder mergeCommand = new StringBuilder($"MERGE INTO {entity.FullTableName} WITH (HOLDLOCK) AS Target USING {tempTableName} AS SOURCE ");
            bool isFirst = true;
            foreach (IProperty property in entity.PrimaryKeyProperties)
            {
                if (isFirst)
                {
                    mergeCommand.Append($"ON [Target].[{property.Name}] = [Source].[{property.Name}] ");
                    isFirst = false;
                }
                else
                {
                    mergeCommand.Append($"AND [Target].[{property.Name}] = [Source].[{property.Name}] ");
                }
            }

            // Update command looks like: 
            // WHEN MATCHED THEN UPDATE SET[Target].[Id] = [Source].[Id], [Target].[ProjectId] = [Source].[ProjectId], [Target].[CreatedDate] = [Source].[CreatedDate]
            isFirst = true;
            StringBuilder updateCommand = new StringBuilder();
            foreach (IProperty property in entity.EntityProperties)
            {
                if (isFirst)
                {
                    updateCommand.Append($"WHEN MATCHED THEN UPDATE SET [Target].[{property.Name}] = [Source].[{property.Name}]");
                    isFirst = false;
                }
                else
                {
                    updateCommand.Append($", [Target].[{property.Name}] = [Source].[{property.Name}]");
                }
            }

            // Insert command looks like: 
            // WHEN NOT MATCHED BY TARGET THEN INSERT ([Id], [ProjectId], [CreatedDate]) VALUES ([Source].[Id], [Source].[ProjectId], [Source].[CreatedDate]
            isFirst = true;
            StringBuilder insertCommand = new StringBuilder();
            foreach (IProperty property in entity.EntityProperties)
            {
                if (isFirst)
                {
                    insertCommand.Append($" WHEN NOT MATCHED BY TARGET THEN INSERT ([{property.Name}]");
                    isFirst = false;
                }
                else
                {
                    insertCommand.Append($", [{property.Name}]");
                }
            }
            isFirst = true;
            foreach (IProperty property in entity.EntityProperties)
            {
                if (isFirst)
                {
                    insertCommand.Append($") VALUES ([Source].[{property.Name}]");
                    isFirst = false;
                }
                else
                {
                    insertCommand.Append($", [Source].[{property.Name}]");
                }
            }
            insertCommand.Append(") ");

            // Delete command looks like:
            // WHEN MATCHED THEN DELETE
            string deleteCommand = "WHEN MATCHED THEN DELETE";

            // Finally construct merge operation based action type!
            StringBuilder outCommand = new StringBuilder();
            if (operationType == OperationType.Insert)
            {
                outCommand.Append(mergeCommand);
                outCommand.Append(insertCommand);
            }
            else if (operationType == OperationType.Update)
            {
                outCommand.Append(mergeCommand);
                outCommand.Append(updateCommand);
            }
            else if (operationType == OperationType.InsertOrUpdate)
            {
                outCommand.Append(mergeCommand);
                outCommand.Append(updateCommand);
                outCommand.Append(insertCommand);
            }
            else if (operationType == OperationType.Delete)
            {
                outCommand.Append(mergeCommand);
                outCommand.Append(deleteCommand);
            }

            // Always drop temp table
            outCommand.Append($"; DROP TABLE {tempTableName};");

            return outCommand.ToString();
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

        /// <summary>
        /// Output command looks like below after command is fully constructed
        /// SELECT TOP 0 Source.[RepoId] INTO [_VssRepository_f505b2] FROM [VssRepository] AS A LEFT JOIN [VssRepository] AS Source ON 1 = 2
        /// </summary>
        private static string BuildStagingTableCommand(EntityMapping mapping, string tableName, OperationType operationType)
        {
            IEnumerable<IProperty> source = BulkOperationHelper.GetPropertiesByOperation(mapping, operationType);
            StringBuilder stagingTableCommand = new StringBuilder();
            bool isFirst = true;

            stagingTableCommand.Append($"SELECT TOP 0 ");
            foreach (IProperty prop in source)
            {
                if (isFirst)
                {
                    stagingTableCommand.Append($"Source.[{prop.GetColumnName()}]");
                    isFirst = false;
                }
                else
                {
                    stagingTableCommand.Append($", Source.[{prop.GetColumnName()}]");
                }
            }
            stagingTableCommand.Append($" INTO {tableName} FROM {mapping.FullTableName} AS A LEFT JOIN {mapping.FullTableName} AS Source ON 1 = 2");

            return stagingTableCommand.ToString();
        }

        private static void BulkInsertToTable<TEntity>(EntityMapping entityMapping, DbContext context, IEnumerable<TEntity> entities, string tableName, OperationType operationType) where TEntity : class
        {
            IEnumerable<IProperty> props = BulkOperationHelper.GetPropertiesByOperation(entityMapping, operationType);

            using SqlBulkCopy sqlBulkCopy = new SqlBulkCopy((SqlConnection)context.Database.GetDbConnection(), SqlBulkCopyOptions.Default, (SqlTransaction)context.Database.CurrentTransaction?.GetDbTransaction());
            foreach (IProperty prop in props)
            {
                sqlBulkCopy.ColumnMappings.Add(prop.GetColumnName(), prop.GetColumnName());
            }
            sqlBulkCopy.BatchSize = 5000;
            sqlBulkCopy.DestinationTableName = tableName;
            sqlBulkCopy.BulkCopyTimeout = context.Database.GetCommandTimeout().Value;
            ObjectReader reader = ObjectReader.Create(entities);
            sqlBulkCopy.WriteToServer(reader);
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
    }
}