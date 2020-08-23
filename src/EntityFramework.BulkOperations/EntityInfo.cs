using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Collections.Generic;
using System.Linq;

namespace EntityFramework.BulkOperations
{
    internal class EntityInfo
    {
        private readonly IEntityType entityType;

        internal EntityInfo(IEntityType entityType)
        {
            this.entityType = entityType;
        }

        internal string TableName => this.entityType.GetTableName();

        internal string Schema => this.entityType.GetSchema();

        internal string FullTableName => string.IsNullOrEmpty(this.entityType.GetSchema()?.Trim()) ? $"[{this.entityType.GetTableName()}]" : $"[{this.entityType.GetSchema()}].[{this.entityType.GetTableName()}]";

        internal IEnumerable<IProperty> EntityProperties => this.entityType.GetProperties();

        internal IEnumerable<IProperty> PrimaryKeyProperties => this.entityType.GetProperties().Where(v => v.IsPrimaryKey());
    }
}