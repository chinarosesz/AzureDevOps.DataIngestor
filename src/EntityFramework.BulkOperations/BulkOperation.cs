using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace EntityFramework.BulkOperations
{
    public static class BulkOperation
    {
        /// <summary>
        /// Bulk insert a collection of objects into the database.
        /// </summary>
        /// <returns>The number of affected rows.</returns>
        public static int BulkInsert<TEntity>(this DbContext context, IEnumerable<TEntity> entities) where TEntity : class
        {
            int commitedResult = BulkOperationHelper.CommitTransaction(context, entities, OperationType.Insert);
            return commitedResult;
        }

        /// <summary>
        /// Bulk upate a collection of objects into the database
        /// </summary>
        /// <returns>The number of affected rows</returns>
        public static int BulkUpdate<TEntity>(this DbContext context, IEnumerable<TEntity> entities) where TEntity : class
        {
            int commitedResult = BulkOperationHelper.CommitTransaction(context, entities, OperationType.Update);
            return commitedResult;
        }

        /// <summary>
        /// Bulk insert or update a collection of entities into database and returns the number of affected rows
        /// </summary>
        public static int BulkInsertOrUpdate<TEntity>(this DbContext context, IEnumerable<TEntity> entities) where TEntity : class
        {
            int commitedResult = BulkOperationHelper.CommitTransaction(context, entities, OperationType.InsertOrUpdate);
            return commitedResult;
        }

        /// <summary>
        /// Bulk delete a collection of objects from the database.
        /// </summary>
        /// <returns>The number of affected rows.</returns>
        public static int BulkDelete<TEntity>(this DbContext context, IEnumerable<TEntity> entities) where TEntity : class
        {
            int commitedResult = BulkOperationHelper.CommitTransaction(context, entities, OperationType.Delete);
            return commitedResult;
        }
    }
}