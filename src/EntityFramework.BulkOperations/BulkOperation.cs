using EntityFramework.BulkExtensions.Commons.Context;
using EntityFramework.BulkExtensions.Commons.Helpers;
using EntityFramework.BulkExtensions.Commons.Mapping;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

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

            //if (hierarchy.Any())
            //{
            //    entityMapping.HierarchyMapping = GetHierarchyMappings(hierarchy);
            //    properties.Add(new PropertyMapping
            //    {
            //        ColumnName = entityType.GetDiscriminatorProperty().Name,
            //        IsHierarchyMapping = true
            //    });
            //}

            entityMapping.Properties = properties;
            return entityMapping;
        }

        private static Dictionary<string, string> GetHierarchyMappings(IEnumerable<IEntityType> hierarchy)
        {
            return hierarchy.ToDictionary(entityType => entityType.ClrType.Name, entityType => entityType.GetDiscriminatorValue() as string);
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

        private static int CommitTransaction<TEntity>(DbContext dbContext, IEnumerable<TEntity> collection, OperationType operation, BulkOptions options = BulkOptions.Default) where TEntity : class
        {
            DbContextWrapper context = new DbContextWrapper(dbContext, GetMapping<TEntity>(dbContext));
            string text = SqlHelper.RandomTableName(context.EntityMapping);
            bool flag = SqlHelper.WillOutputGeneratedValues(context.EntityMapping, options);

            List<TEntity> list = collection.ToList();
            if (!list.Any())
            {
                return list.Count;
            }
            try
            {
                string text2 = flag ? SqlHelper.RandomTableName(context.EntityMapping) : null;
                List<PropertyMapping> list2 = flag ? BulkOperation.GetPropertiesByOptions(context.EntityMapping, options).ToList() : null;
                string text3 = SqlHelper.BuildStagingTableCommand(context.EntityMapping, text, operation, options);
                if (string.IsNullOrEmpty(text3))
                {
                    context.Rollback();
                    return 0;
                }

                context.ExecuteSqlCommand(text3);
                BulkOperation.BulkInsertToTable(context, list, text, operation, options);
                if (flag)
                {
                    context.ExecuteSqlCommand(SqlHelper.BuildOutputTableCommand(text2, context.EntityMapping, list2));
                }

                string str = SqlHelper.BuildMergeCommand(context, text, operation);
                if (flag)
                {
                    str += SqlHelper.BuildMergeOutputSet(text2, list2);
                }
                
                str += SqlHelper.GetDropTableCommand(text);
                int result = context.ExecuteSqlCommand(str);
                if (flag)
                {
                    SqlHelper.LoadFromOutputTable(context, text2, list2, list);
                }
                context.Commit();
                return result;
            }
            catch (Exception)
            {
                context.Rollback();
                throw;
            }
        }

        private static void BulkInsertToTable<TEntity>(DbContextWrapper context, IList<TEntity> entities, string tableName, OperationType operationType, BulkOptions options) where TEntity : class
        {
            List<PropertyMapping> list = SqlHelper.GetPropertiesByOperation(context.EntityMapping, operationType).ToList();
            if (SqlHelper.WillOutputGeneratedValues(context.EntityMapping, options))
            {
                list.Add(new PropertyMapping
                {
                    ColumnName = "Bulk_Identity",
                    PropertyName = "Bulk_Identity"
                });
            }
            using (SqlBulkCopy sqlBulkCopy = new SqlBulkCopy((SqlConnection)context.Connection, SqlBulkCopyOptions.Default, (SqlTransaction)context.Transaction))
            {
                foreach (PropertyMapping item in list)
                {
                    sqlBulkCopy.ColumnMappings.Add(item.ColumnName, item.ColumnName);
                }
                sqlBulkCopy.BatchSize = context.BatchSize;
                sqlBulkCopy.DestinationTableName = tableName;
                sqlBulkCopy.BulkCopyTimeout = context.Timeout;
                sqlBulkCopy.WriteToServer((IDataReader)DataReaderHelper.ToDataReader(entities, context.EntityMapping, list));
            }
        }

        internal static IEnumerable<PropertyMapping> GetPropertiesByOptions(EntityMapping mapping, BulkOptions options)
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
    }
}