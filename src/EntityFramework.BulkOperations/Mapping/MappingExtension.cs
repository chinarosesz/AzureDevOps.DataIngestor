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
            var entityType = context.Model.FindEntityType(typeof(TEntity));
            var relational = entityType.Relational();
            var baseType = entityType.BaseType ?? entityType;
            var hierarchy = context.Model.GetEntityTypes()
                .Where(type => type.BaseType == null ? type == baseType : type.BaseType == baseType)
                .ToList();
            var properties = hierarchy.GetPropertyMappings().ToList();

            var entityMapping = new EntityMapping
            {
                TableName = relational.TableName,
                Schema = relational.Schema
            };

            if (hierarchy.Any())
            {
                entityMapping.HierarchyMapping = GetHierarchyMappings(hierarchy);
                properties.Add(new PropertyMapping
                {
                    ColumnName = relational.DiscriminatorProperty.Name,
                    IsHierarchyMapping = true
                });
            }

            entityMapping.Properties = properties;
            return entityMapping;
        }

        private static Dictionary<string, string> GetHierarchyMappings(IEnumerable<IEntityType> hierarchy)
        {
            return hierarchy.ToDictionary(entityType => entityType.ClrType.Name,
                entityType => entityType.Relational().DiscriminatorValue.ToString());
        }

        private static IEnumerable<IPropertyMapping> GetPropertyMappings(this IEnumerable<IEntityType> hierarchy)
        {
            return hierarchy
                .SelectMany(type => type.GetProperties().Where(property => !property.IsShadowProperty))
                .Distinct()
                .ToList()
                .Select(property => new PropertyMapping
                {
                    PropertyName = property.Name,
                    ColumnName = property.Relational().ColumnName,
                    IsPk = property.IsPrimaryKey()
                });
        }
    }
}