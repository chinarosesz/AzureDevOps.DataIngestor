using EntityFramework.BulkExtensions.Commons.Context;
using EntityFramework.BulkExtensions.Commons.Helpers;
using EntityFramework.BulkExtensions.Commons.Mapping;
using Microsoft.Data.SqlClient;
using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EntityFramework.BulkOperations
{
    internal class BulkActions
    {
        internal static int BulkDelete<TEntity>(DbContextWrapper context, IEnumerable<TEntity> collection) where TEntity : class
        {
            string tmpTableName = SqlHelper.RandomTableName(context.EntityMapping);
            List<TEntity> entityList = collection.ToList();
            if (!entityList.Any())
            {
                return entityList.Count;
            }

            try
            {
                // Create temporary table with only the primary keys.
                string tempTableQueryString = SqlHelper.CreateTempTableQueryString(context.EntityMapping, tmpTableName, OperationType.Delete);
                context.ExecuteSqlCommand(tempTableQueryString);

                // Bulk inset data to temporary table.
                BulkInsertToTable(context, entityList, tmpTableName, OperationType.Delete);

                // Merge delete items from the target table that matches ids from the temporary table.
                string deleteSqlString = SqlHelper.BuildDeleteCommand(context, tmpTableName);
                int affectedRows = context.ExecuteSqlCommand(deleteSqlString);

                // Commit if internal transaction exists.
                context.Commit();
                return affectedRows;
            }
            catch (Exception)
            {
                // Rollback if internal transaction exists.
                context.Rollback();
                throw;
            }
        }

        internal static int BulkInsert<TEntity>(DbContextWrapper context, IEnumerable<TEntity> collection, BulkOptions options) where TEntity : class
        {
            List<TEntity> entityList = collection.ToList();
            if (!entityList.Any())
            {
                return entityList.Count;
            }

            try
            {
                // Return generated IDs for bulk inserted elements.
                if (options.HasFlag(BulkOptions.OutputIdentity))
                {
                    // Create temporary table.
                    string tmpTableName = SqlHelper.RandomTableName(context.EntityMapping);
                    string tempTableQueryString = SqlHelper.CreateTempTableQueryString(context.EntityMapping, tmpTableName, OperationType.Insert);
                    context.ExecuteSqlCommand(tempTableQueryString);

                    // Bulk inset data to temporary temporary table.
                    BulkInsertToTable(context, entityList, tmpTableName, OperationType.Insert);

                    // Copy data from temporary table to destination table with ID output to another temporary table.
                    string tmpOutputTableName = SqlHelper.RandomTableName(context.EntityMapping);
                    string commandText = SqlHelper.GetInsertIntoStagingTableCmd(context.EntityMapping, tmpOutputTableName, tmpTableName, context.EntityMapping.Pks.First().ColumnName);
                    context.ExecuteSqlCommand(commandText);

                    // Load generated IDs from temporary output table into the entities.
                    SqlHelper.LoadFromTmpOutputTable(context, tmpOutputTableName, context.EntityMapping.Pks.First(), entityList);
                }
                else
                {
                    //Bulk inset data to temporary destination table.
                    BulkInsertToTable(context, entityList, context.EntityMapping.FullTableName, OperationType.Insert);
                }

                //Commit if internal transaction exists.
                context.Commit();
                return entityList.Count;
            }
            catch (Exception)
            {
                //Rollback if internal transaction exists.
                context.Rollback();
                throw;
            }
        }

        internal static int BulkUpdate<TEntity>(DbContextWrapper context, IEnumerable<TEntity> collection) where TEntity : class
        {
            List<TEntity> entityList = collection.ToList();
            if (!entityList.Any())
            {
                return entityList.Count;
            }

            try
            {
                // Create temporary table.
                string tmpTableName = SqlHelper.RandomTableName(context.EntityMapping);
                string tempTableQueryString = SqlHelper.CreateTempTableQueryString(context.EntityMapping, tmpTableName, OperationType.Update);
                context.ExecuteSqlCommand(tempTableQueryString);

                // Bulk insert data to temporary temporary table.
                BulkInsertToTable(context, entityList, tmpTableName, OperationType.Update);

                // Copy data from temporary table to destination table.
                int affectedRows = context.ExecuteSqlCommand(SqlHelper.BuildMergeCommand(context, tmpTableName));

                // Commit if internal transaction exists.
                context.Commit();
                return affectedRows;
            }
            catch (Exception)
            {
                // Rollback if internal transaction exists.
                context.Rollback();
                throw;
            }
        }

        internal static int BulkInsertOrUpdate<TEntity>(DbContextWrapper context, IEnumerable<TEntity> collection) where TEntity : class
        {
            List<TEntity> entityList = collection.ToList();
            if (!entityList.Any())
            {
                return entityList.Count;
            }

            try
            {
                // Create temporary table.
                string tempTableName = SqlHelper.RandomTableName(context.EntityMapping);
                string tempTableQueryString = SqlHelper.CreateTempTableQueryString(context.EntityMapping, tempTableName, OperationType.Update);
                context.ExecuteSqlCommand(tempTableQueryString);

                // Bulk insert data to temporary temporary table.
                BulkInsertToTable(context, entityList, tempTableName, OperationType.InsertOrUpdate);

                // Copy data from temporary table to destination table.
                int affectedRows = context.ExecuteSqlCommand(SqlHelper.BuildInsertOrUpdateCommand(context, tempTableName));

                // Commit if internal transaction exists.
                context.Commit();
                return affectedRows;
            }
            catch (Exception)
            {
                // Rollback if internal transaction exists.
                context.Rollback();
                throw;
            }
        }

        private static void BulkInsertToTable<TEntity>(DbContextWrapper context, IEnumerable<TEntity> entities, string tableName, OperationType operationType) where TEntity : class
        {
            EnumerableDataReader dataReader = SqlHelper.ToDataReader(entities, context.EntityMapping);

            IEnumerable<PropertyMapping> filteredProps = SqlHelper.FilterProperties(context.EntityMapping.Properties, operationType);

            using SqlBulkCopy bulkcopy = new SqlBulkCopy((SqlConnection)context.Connection, SqlBulkCopyOptions.Default | SqlBulkCopyOptions.KeepIdentity, (SqlTransaction)context.Transaction);
            foreach (PropertyMapping column in filteredProps)
            {
                bulkcopy.ColumnMappings.Add(column.ColumnName, column.ColumnName);
            }
            bulkcopy.DestinationTableName = tableName;
            bulkcopy.BulkCopyTimeout = 1000;
            bulkcopy.WriteToServer(dataReader);
        }
    }
}
