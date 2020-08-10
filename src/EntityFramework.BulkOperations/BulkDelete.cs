using EntityFramework.BulkExtensions.Commons.Context;
using EntityFramework.BulkExtensions.Commons.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EntityFramework.BulkExtensions.Commons.BulkOperations
{
    internal class BulkDelete : IBulkOperation
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
                // Create temporary table with only the primary keys.
                string tempTableQueryString = SqlHelper.CreateTempTableQueryString(context.EntityMapping, tmpTableName, OperationType.Delete);
                context.ExecuteSqlCommand(tempTableQueryString);

                // Bulk inset data to temporary table.
                SqlHelper.BulkInsertToTable(context, entityList, tmpTableName, OperationType.Delete);

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
    }
}