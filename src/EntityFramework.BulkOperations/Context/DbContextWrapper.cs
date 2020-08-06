using System;
using System.Collections.Generic;
using System.Data;
using EntityFramework.BulkExtensions.Commons.Mapping;

namespace EntityFramework.BulkExtensions.Commons.Context
{
    internal class DbContextWrapper : IDbContextWrapper
    {
        internal DbContextWrapper(IDbConnection connection, IDbTransaction transaction, IEntityMapping entityMapping)
        {
            Connection = connection;
            if (Connection.State != ConnectionState.Open)
                Connection.Open();

            IsInternalTransaction = transaction == null;
            Transaction = transaction ?? connection.BeginTransaction();
            EntityMapping = entityMapping;
        }

        public IEntityMapping EntityMapping { get; }
        public IDbConnection Connection { get; }
        public IDbTransaction Transaction { get; }
        private bool IsInternalTransaction { get; }

        public int ExecuteSqlCommand(string command)
        {
            var sqlCommand = Connection.CreateCommand();
            sqlCommand.Transaction = Transaction;
            sqlCommand.CommandTimeout = Connection.ConnectionTimeout;
            sqlCommand.CommandText = command;

            return sqlCommand.ExecuteNonQuery();
        }

        public IEnumerable<T> SqlQuery<T>(string command) where T : struct
        {
            var list = new List<T>();
            var sqlCommand = Connection.CreateCommand();
            sqlCommand.Transaction = Transaction;
            sqlCommand.CommandTimeout = Connection.ConnectionTimeout;
            sqlCommand.CommandText = command;

            using (var reader = sqlCommand.ExecuteReader())
            {
                while (reader.Read())
                {
                    if (reader.FieldCount > 1)
                        throw new Exception("The select command must have one column only");
                    list.Add((T)reader.GetValue(0));
                }
            }

            return list;
        }

        public void Commit()
        {
            if (IsInternalTransaction)
                Transaction.Commit();
        }

        public void Rollback()
        {
            if (IsInternalTransaction)
                Transaction.Rollback();
        }
    }
}