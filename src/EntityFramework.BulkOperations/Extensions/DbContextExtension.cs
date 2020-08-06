using EntityFramework.BulkExtensions.Commons.Context;
using EntityFrameworkCore.BulkExtensions.Mapping;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace EntityFrameworkCore.BulkExtensions.Extensions
{
    internal static class DbContextExtension
    {
        internal static DbContextWrapper GetContextWrapper<TEntity>(this DbContext context) where TEntity : class
        {
            var contextTransactionManager = context.GetInfrastructure().GetService<IDbContextTransactionManager>();
            var relationalConnection = (IRelationalConnection) contextTransactionManager;
            var connection = relationalConnection.DbConnection;
            var transaction = relationalConnection.CurrentTransaction?.GetDbTransaction();

            return new DbContextWrapper(connection, transaction, context.Mapping<TEntity>());
        }
    }
}