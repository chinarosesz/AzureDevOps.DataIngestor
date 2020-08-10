using System;
using System.Collections.Generic;
using System.Data;
using EntityFramework.BulkExtensions.Commons.Mapping;

namespace EntityFramework.BulkExtensions.Commons.Context
{
    internal class DbContextWrapper
    {
        public EntityMapping EntityMapping { get; }
        public IDbConnection Connection { get; }
        public IDbTransaction Transaction { get; }
        private bool IsInternalTransaction { get; }

        internal DbContextWrapper(IDbConnection connection, IDbTransaction transaction, EntityMapping entityMapping)
        {
            this.Connection = connection;
            if (Connection.State != ConnectionState.Open) { Connection.Open(); }

            IsInternalTransaction = transaction == null;
            Transaction = transaction ?? connection.BeginTransaction();
            EntityMapping = entityMapping;
        }

        public int ExecuteSqlCommand(string command)
        {
            IDbCommand sqlCommand = Connection.CreateCommand();
            sqlCommand.Transaction = Transaction;
            sqlCommand.CommandTimeout = Connection.ConnectionTimeout;
            sqlCommand.CommandText = command;

            return sqlCommand.ExecuteNonQuery();
        }

        public IEnumerable<T> SqlQuery<T>(string command) where T : struct
        {
            List<T> list = new List<T>();
            IDbCommand sqlCommand = Connection.CreateCommand();
            sqlCommand.Transaction = Transaction;
            sqlCommand.CommandTimeout = Connection.ConnectionTimeout;
            sqlCommand.CommandText = command;

            using (IDataReader reader = sqlCommand.ExecuteReader())
            {
                while (reader.Read())
                {
                    if (reader.FieldCount > 1) 
                    { 
                        throw new Exception("The select command must have one column only"); 
                    }

                    list.Add((T)reader.GetValue(0));
                }
            }

            return list;
        }

        public void Commit()
        {
            if (IsInternalTransaction) 
            { 
                Transaction.Commit(); 
            }
        }

        public void Rollback()
        {
            if (IsInternalTransaction)
            {
                Transaction.Rollback();
            }
        }
    }
}