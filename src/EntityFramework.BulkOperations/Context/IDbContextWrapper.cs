using System.Collections.Generic;
using System.Data;
using EntityFramework.BulkExtensions.Commons.Mapping;

namespace EntityFramework.BulkExtensions.Commons.Context
{
    internal interface IDbContextWrapper
    {
        IDbConnection Connection { get; }
        IDbTransaction Transaction { get; }
        IEntityMapping EntityMapping { get; }

        int ExecuteSqlCommand(string command);

        IEnumerable<T> SqlQuery<T>(string command) where T : struct;

        void Commit();

        void Rollback();
    }
}