using EntityFramework.BulkExtensions.Commons.BulkOperations;
using EntityFramework.BulkExtensions.Commons.Context;
using EntityFramework.BulkExtensions.Commons.Helpers;
using EntityFramework.BulkExtensions.Commons.Mapping;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using System.Collections.Generic;
using System.Linq;

namespace EntityFrameworkCore.BulkExtensions
{
    public static class BulkExtensions
    {
        /// <summary>
        /// Bulk insert a collection of objects into the database.
        /// </summary>
        /// <param name="context">The EntityFramework DbContext object.</param>
        /// <param name="entities">The collection of objects to be inserted.</param>
        /// <param name="option"></param>
        /// <typeparam name="TEntity">The type of the objects collection. TEntity must be a class.</typeparam>
        /// <returns>The number of affected rows.</returns>
        public static int BulkInsert<TEntity>(this DbContext context, IEnumerable<TEntity> entities, BulkOptions bulkOptions = BulkOptions.Default) where TEntity : class
        {
            DbContextWrapper contextWrapper = new DbContextWrapper(context.Database.GetDbConnection(), context.Database.CurrentTransaction.GetDbTransaction(), GetMapping<TEntity>(context));
            IBulkOperation bulkOperation = new BulkInsert();
            int commitedResult = bulkOperation.CommitTransaction(contextWrapper, entities, bulkOptions);
            return commitedResult;
        }

        /// <summary>
        /// Bulk update a collection of objects into the database.
        /// </summary>
        /// <param name="context">The EntityFramework DbContext object.</param>
        /// <param name="entities">The collection of objects to be updated.</param>
        /// <typeparam name="TEntity">The type of the objects collection. TEntity must be a class.</typeparam>
        /// <returns>The number of affected rows.</returns>
        public static int BulkUpdate<TEntity>(this DbContext context, IEnumerable<TEntity> entities) where TEntity : class
        {
            DbContextWrapper contextWrapper = new DbContextWrapper(context.Database.GetDbConnection(), context.Database.CurrentTransaction.GetDbTransaction(), GetMapping<TEntity>(context));
            IBulkOperation bulkOperation = new BulkUpdate();
            int commitedResult = bulkOperation.CommitTransaction(contextWrapper, entities);
            return commitedResult;
        }

        /// <summary>
        /// Bulk delete a collection of objects from the database.
        /// </summary>
        /// <param name="context">The EntityFramework DbContext object.</param>
        /// <param name="entities">The collection of objects to be deleted.</param>
        /// <typeparam name="TEntity">The type of the objects collection. TEntity must be a class.</typeparam>
        /// <returns>The number of affected rows.</returns>
        public static int BulkDelete<TEntity>(this DbContext context, IEnumerable<TEntity> entities) where TEntity : class
        {
            DbContextWrapper contextWrapper = new DbContextWrapper(context.Database.GetDbConnection(), context.Database.CurrentTransaction.GetDbTransaction(), GetMapping<TEntity>(context));
            IBulkOperation bulkOperation = new BulkDelete();
            int commitedResult = bulkOperation.CommitTransaction(contextWrapper, entities);
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
    }
}