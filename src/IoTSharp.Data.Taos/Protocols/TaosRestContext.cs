﻿using IoTSharp.Data.Taos.Driver;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using TDengineDriver;

namespace IoTSharp.Data.Taos.Protocols
{
    internal class TaosRestContext : ITaosContext
    {
        private readonly TaosResult tr;

        public int AffectRows { get; set; }
        public int FieldCount { get; set; }

        private List<List<string>> _meta;
        JArray _data;
        int _data_index=-1;
        private JToken _dr;

        public bool CloseConnection { get; set; }

        public TaosRestContext(TaosResult tr)
        {
            this.tr = tr;
            AffectRows = tr.rows;
            FieldCount = tr.column_meta.Count;
            _meta = tr.column_meta;
            _data = tr.data as JArray;
            _data_index = -1;
            Debug.Assert(tr.rows > 0 && _data != null && _data.Count > 0,"data is empty!");
        }
        public bool HaveData => _data_index+1 >= 0 && _data_index+1 < _data?.Count;
        public bool Read()
        {
            _data_index++;
            var havedata = _data_index >= 0 && _data_index < _data?.Count;
            if (havedata)
            {
                 _dr = _data[_data_index];
            }
            return havedata;
        }

        public TaosException LastException()
        {
            return new TaosException(new TaosErrorResult() { Code = 0, Error = "" });
        }

        public int GetErrorNo()
        {
            return 0;
        }

        public void Dispose()
        {
         
        }

        public bool GetBoolean(int ordinal)
        {
            return _dr[ordinal].Value<bool>();
        }

        public byte GetByte(int ordinal)
        {
            return _dr[ordinal].Value<byte>();
        }

        public long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length)
        {
            var bytes = _dr[ordinal].Value<byte[]>();
            Array.Copy(bytes, bufferOffset, buffer,  dataOffset, length);
            return length;
        }

        public DateTime GetDataTime(int ordinal)
        {
            return _dr[ordinal].Value<DateTime>();
        }

        public DataTable GetSchemaTable(Func<int, string> getName, Func<int, Type> getFieldType, Func<int, string> getDataTypeName, TaosCommand _command)
        {
            return null;
        }

        public Stream GetStream(int ordinal)
        {
             return new MemoryStream( _dr[ordinal].Value<byte[]>());
        }

        public TimeSpan GetTimeSpan(int ordinal)
        {
            return _dr[ordinal].Value<TimeSpan>();
        }

        public object GetValue(int ordinal)
        {
            return ((JValue)_dr[ordinal]).Value;
        }

        public Type GetFieldType(int ordinal)
        {
            var strType = _meta[ordinal][1];
            var tdtype = TDengineDataType.TSDB_DATA_TYPE_NULL;
            switch (strType)
            {
                case "BOOL":
                    tdtype = TDengineDataType.TSDB_DATA_TYPE_BOOL;
                    break;
                case "TINYINT":
                    tdtype = TDengineDataType.TSDB_DATA_TYPE_TINYINT;
                    break;
                case "SMALLINT":
                    tdtype = TDengineDataType.TSDB_DATA_TYPE_SMALLINT;
                    break;
                case "INT":
                    tdtype = TDengineDataType.TSDB_DATA_TYPE_INT;
                    break;
                case "BIGINT":
                    tdtype = TDengineDataType.TSDB_DATA_TYPE_BIGINT;
                    break;
                case "TINYINT UNSIGNED":
                    tdtype = TDengineDataType.TSDB_DATA_TYPE_UTINYINT;
                    break;
                case "SMALLINT UNSIGNED":
                    tdtype = TDengineDataType.TSDB_DATA_TYPE_USMALLINT;
                    break;
                case "INT UNSIGNED":
                    tdtype = TDengineDataType.TSDB_DATA_TYPE_UINT;
                    break;
                case "BIGINT UNSIGNED":
                    tdtype = TDengineDataType.TSDB_DATA_TYPE_UBIGINT;
                    break;
                case "FLOAT":
                    tdtype = TDengineDataType.TSDB_DATA_TYPE_FLOAT;
                    break;
                case "DOUBLE":
                    tdtype = TDengineDataType.TSDB_DATA_TYPE_DOUBLE;
                    break;
                case "TIMESTAMP":
                    tdtype = TDengineDataType.TSDB_DATA_TYPE_TIMESTAMP;
                    break;
                case "NCHAR":
                    tdtype = TDengineDataType.TSDB_DATA_TYPE_NCHAR;
                    break;
                case "JSON":
                    tdtype = TDengineDataType.TSDB_DATA_TYPE_JSONTAG;
                    break;
                case "VARCHAR":
                    tdtype = TDengineDataType.TSDB_DATA_TYPE_VARCHAR;
                    break;
                case "DECIMAL":
                    tdtype = TDengineDataType.TSDB_DATA_TYPE_DECIMAL;
                    break;
                case "BLOB":
                    tdtype = TDengineDataType.TSDB_DATA_TYPE_BLOB;
                    break;
                case "MEDIUMBLOB":
                    tdtype = TDengineDataType.TSDB_DATA_TYPE_MEDIUMBLOB;
                    break;
                default:
                    tdtype = TDengineDataType.TSDB_DATA_TYPE_NULL;
                    break;
            }
            return tdtype.ToCrlType();
        }

        public string GetName(int ordinal)
        {
          return  _meta[ordinal][0];
        }

        public int GetOrdinal(string name)
        {
            return _meta.IndexOf( _meta.FirstOrDefault(f=>f[0]==name));
        }
    }
}
