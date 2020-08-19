using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Collections.Generic;
using System.Linq;

namespace EntityFramework.BulkOperations
{
    public class EntityMapping
    {
        public EntityMapping(IEntityType entityType)
        {
            this.TableName = entityType.GetTableName();
            this.Schema = entityType.GetSchema();
            this.EntityType = entityType;
            this.FullTableName = string.IsNullOrEmpty(this.Schema?.Trim()) ? $"[{TableName}]" : $"[{Schema}].[{TableName}]";
            this.EntityProperties = this.EntityType.GetProperties();
            this.PrimaryKeyProperties = this.EntityType.GetProperties().Where(v => v.IsKey());
        }

        public IEntityType EntityType { get; }

        public string TableName { private set;  get; }
        
        public string Schema { private set; get; }

        public string FullTableName { private set; get; }

        public IEnumerable<IProperty> EntityProperties { private set; get; }

        public IEnumerable<IProperty> PrimaryKeyProperties { private set; get; }
    }
}