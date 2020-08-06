using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace Shared.Helpers
{
    internal class EnumerableDataReader : DbDataReader
    {
        private object[] _currentElement;
        private readonly IList<object[]> _collection;
        private readonly IList<string> _columns;
        private readonly IEnumerator _enumerator;
        private readonly IList<Guid> _columnGuids;

        public EnumerableDataReader(IEnumerable<string> columns, IEnumerable<object[]> collection)
        {
            _columns = columns.ToList();
            _collection = collection.ToList();
            _enumerator = _collection.GetEnumerator();
            _enumerator.Reset();
            _columnGuids = new List<Guid>();
            foreach (var unused in _columns)
            {
                _columnGuids.Add(Guid.NewGuid());
            }
        }

#if !NETSTANDARD1_3

        public override void Close()
        {
            throw new NotImplementedException();
        }

        public override DataTable GetSchemaTable()
        {
            throw new NotImplementedException();
        }

#endif

        public override bool NextResult()
        {
            var moved = _enumerator.MoveNext();
            if (moved)
                _currentElement = _enumerator.Current as object[];
            return moved;
        }

        public override bool Read()
        {
            var moved = _enumerator.MoveNext();
            if (moved)
                _currentElement = _enumerator.Current as object[];
            return moved;
        }

        public override int Depth => 0;
        public override bool IsClosed => false;
        public override int RecordsAffected => -1;

        public override bool GetBoolean(int ordinal)
        {
            return (bool) _currentElement[ordinal];
        }

        public override byte GetByte(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length)
        {
            throw new NotImplementedException();
        }

        public override char GetChar(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length)
        {
            throw new NotImplementedException();
        }

        public override Guid GetGuid(int ordinal)
        {
            return _columnGuids[ordinal];
        }

        public override short GetInt16(int ordinal)
        {
            return (short) _currentElement[ordinal];
        }

        public override int GetInt32(int ordinal)
        {
            return (int) _currentElement[ordinal];
        }

        public override long GetInt64(int ordinal)
        {
            return (long) _currentElement[ordinal];
        }

        public override DateTime GetDateTime(int ordinal)
        {
            return (DateTime) _currentElement[ordinal];
        }

        public override string GetString(int ordinal)
        {
            return _currentElement[ordinal].ToString();
        }

        public override decimal GetDecimal(int ordinal)
        {
            return (decimal) _currentElement[ordinal];
        }

        public override double GetDouble(int ordinal)
        {
            return (double) _currentElement[ordinal];
        }

        public override float GetFloat(int ordinal)
        {
            return (float) _currentElement[ordinal];
        }

        public override string GetName(int ordinal)
        {
            return _columns[ordinal];
        }

        public override int GetValues(object[] values)
        {
            throw new NotImplementedException();
        }

        public override bool IsDBNull(int ordinal)
        {
            return _currentElement[ordinal] == null;
        }

        public override int FieldCount => _collection.First().Length;

        public override object this[int ordinal] => _currentElement;

        public override object this[string name]
        {
            get
            {
                var index = _columns.IndexOf(name);
                return index < 0 ? null : _collection[index];
            }
        }

        public override bool HasRows => _collection.Any();

        public override int GetOrdinal(string name)
        {
            return _columns.IndexOf(name);
        }

        public override string GetDataTypeName(int ordinal)
        {
            return _currentElement[ordinal].GetType().Name;
        }

        public override Type GetFieldType(int ordinal)
        {
            return _currentElement[ordinal].GetType();
        }

        public override object GetValue(int ordinal)
        {
            return _currentElement[ordinal];
        }

        public override IEnumerator GetEnumerator()
        {
            return _collection.GetEnumerator();
        }
    }
}