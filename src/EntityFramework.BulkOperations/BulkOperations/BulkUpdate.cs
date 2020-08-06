using System;
using System.Collections.Generic;
using System.Linq;
using EntityFramework.BulkExtensions.Commons.Context;
using EntityFramework.BulkExtensions.Commons.Extensions;
using EntityFramework.BulkExtensions.Commons.Helpers;

namespace EntityFramework.BulkExtensions.Commons.BulkOperations
{
    /// <summary>
    /// 
    /// </summary>
    internal class BulkUpdate : IBulkOperation
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="collection"></param>
        /// <param name="options"></param>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        int IBulkOperation.CommitTransaction<TEntity>(IDbContextWrapper context, IEnumerable<TEntity> collection, BulkOptions options)
        {
            var tmpTableName = context.EntityMapping.RandomTableName();
            var entityList = collection.ToList();
            if (!entityList.Any())
            {
                return entityList.Count;
            }

            try
            {
                //Create temporary table.
                context.ExecuteSqlCommand(context.EntityMapping.CreateTempTable(tmpTableName, OperationType.Update));                

                //Bulk inset data to temporary temporary table.
                context.BulkInsertToTable(entityList, tmpTableName, OperationType.Update);

                //Copy data from temporary table to destination table.
                var affectedRows = context.ExecuteSqlCommand(context.BuildMergeCommand(tmpTableName));

                //Commit if internal transaction exists.
                context.Commit();
                return affectedRows;
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
