using System.Collections.Generic;
using System.Linq;
using EntityFramework.BulkExtensions.Commons.Mapping;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace EntityFrameworkCore.BulkExtensions.Mapping
{
    internal static class MappingExtension
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="context"></param>
        /// <returns></returns>
        internal static IEntityMapping Mapping<TEntity>(this DbContext context) where TEntity : class
        {
            IEntityType entityType = context.Model.FindEntityType(typeof(TEntity));
            IEntityType baseType = entityType.BaseType ?? entityType;
            List<IEntityType> hierarchy = context.Model.GetEntityTypes()
                .Where(type => type.BaseType == null ? type == baseType : type.BaseType == baseType)
                .ToList();
            List<IPropertyMapping> properties = hierarchy.GetPropertyMappings().ToList();

            EntityMapping entityMapping = new EntityMapping
            {
                TableName = entityType.GetTableName(),
                Schema = entityType.GetSchema(),
            };

            if (hierarchy.Any())
            {
                entityMapping.HierarchyMapping = GetHierarchyMappings(hierarchy);
                properties.Add(new PropertyMapping
                {
                    ColumnName = entityType.GetDiscriminatorProperty().Name,
                    IsHierarchyMapping = true
                });
            }

            entityMapping.Properties = properties;
            return entityMapping;
        }

        private static Dictionary<string, string> GetHierarchyMappings(IEnumerable<IEntityType> hierarchy)
        {
            return hierarchy.ToDictionary(entityType => entityType.ClrType.Name, entityType => entityType.GetDiscriminatorValue() as string);
        }

        private static IEnumerable<IPropertyMapping> GetPropertyMappings(this IEnumerable<IEntityType> hierarchy)
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