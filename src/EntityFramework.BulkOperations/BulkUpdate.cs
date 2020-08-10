using System;
using System.Collections.Generic;
using System.Linq;
using EntityFramework.BulkExtensions.Commons.Context;
using EntityFramework.BulkExtensions.Commons.Helpers;

namespace EntityFramework.BulkExtensions.Commons.BulkOperations
{
    internal class BulkUpdate : IBulkOperation
    {
        int IBulkOperation.CommitTransaction<TEntity>(DbContextWrapper context, IEnumerable<TEntity> collection, BulkOptions options)
        {
            string tmpTableName = SqlHelper.RandomTableName(context.EntityMapping);
            List<TEntity> entityList = collection.ToList();
            if (!entityList.Any())
            {
                return entityList.Count;
            }

            try
            {
                // Create temporary table.
                string tempTableQueryString = SqlHelper.CreateTempTableQueryString(context.EntityMapping, tmpTableName, OperationType.Update);
                context.ExecuteSqlCommand(tempTableQueryString);                

                // Bulk inset data to temporary temporary table.
                SqlHelper.BulkInsertToTable(context, entityList, tmpTableName, OperationType.Update);

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
    }
}
