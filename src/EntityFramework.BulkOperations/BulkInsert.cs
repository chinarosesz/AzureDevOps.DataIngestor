using System;
using System.Collections.Generic;
using System.Linq;
using EntityFramework.BulkExtensions.Commons.Context;
using EntityFramework.BulkExtensions.Commons.Helpers;

namespace EntityFramework.BulkExtensions.Commons.BulkOperations
{
    internal class BulkInsert : IBulkOperation
    {
        int IBulkOperation.CommitTransaction<TEntity>(DbContextWrapper context, IEnumerable<TEntity> collection, BulkOptions options)
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
                    SqlHelper.BulkInsertToTable(context, entityList, tmpTableName, OperationType.Insert);

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
                    SqlHelper.BulkInsertToTable(context, entityList, context.EntityMapping.FullTableName, OperationType.Insert);
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
    }
}