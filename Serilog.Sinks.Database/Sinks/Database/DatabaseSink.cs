using ASql;
using MySql.Data.MySqlClient;
using Org.BouncyCastle.Pqc.Crypto.Utilities;
using Serilog.Core;
using Serilog.Debugging;
using Serilog.Events;

using Serilog.Sinks.Extensions;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using static ASql.ASqlManager;

namespace Serilog.Sinks.Database
{
    internal class DatabaseSink : ILogEventSink
    {
        internal static string sqlCreateTable = "CREATE TABLE {TableName} (id INT NOT NULL IDENTITY PRIMARY KEY,SerTimestamp VARCHAR(100),SerLevel VARCHAR(15),SerTemplate VARCHAR(max),SerMessage VARCHAR(max),SerException VARCHAR(max),SerProperties VARCHAR(max),SerTs DATETIME2(0) DEFAULT GETDATE())";
        internal static string oraCreateTable = "CREATE TABLE {TableName} (id NUMBER(10) NOT NULL PRIMARY KEY,SerTimestamp VARCHAR2(100 CHAR),SerLevel VARCHAR2(15 CHAR),SerTemplate CLOB,SerMessage CLOB,SerException CLOB,SerProperties CLOB,SerTs TIMESTAMP(0) DEFAULT SYSTIMESTAMP)";
        internal static string mysCreateTable = "CREATE TABLE {TableName} (id INT NOT NULL AUTO_INCREMENT PRIMARY KEY,SerTimestamp VARCHAR(100),SerLevel VARCHAR(15),SerTemplate TEXT,SerMessage TEXT,SerException TEXT,SerProperties TEXT,SerTs TIMESTAMP DEFAULT CURRENT_TIMESTAMP)";
        internal static string posCreateTable = "CREATE TABLE {TableName} (id INT NOT NULL GENERATED ALWAYS AS IDENTITY PRIMARY KEY,SerTimestamp VARCHAR(100),SerLevel VARCHAR(15),SerTemplate TEXT,SerMessage TEXT,SerException TEXT,SerProperties TEXT,SerTs TIMESTAMP(0) DEFAULT CURRENT_TIMESTAMP)";
        internal static string litCreateTable = "CREATE TABLE {TableName} (id INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL, SerTimestamp VARCHAR(100),SerLevel VARCHAR(15),SerTemplate TEXT,SerMessage TEXT,SerException TEXT,SerProperties TEXT,SerTs datetime default current_timestamp)";
        internal static string sqlCheckTable = "select case when exists((select * from information_schema.tables where table_name = '{TableName}')) then 1 else 0 end";
        internal static string oraCheckTable = "SELECT table_name FROM USER_TABLES WHERE table_name='{TableName}'";
        internal static string mysCheckTable = "SELECT count(*) as ntab FROM information_schema.TABLES WHERE (TABLE_SCHEMA = 'test') AND (TABLE_NAME = '{TableName}')";
        internal static string posCheckTable = "SELECT table_name FROM information_schema.tables WHERE table_schema='public' and table_name='{TableName}'";
        internal static string litCheckTable = "SELECT name FROM sqlite_master WHERE type='table' AND name='{TableName}'";

        private readonly DBType _dataBaseType;
        private readonly string _connectionString;
        private readonly bool _storeTimestampInUtc;
        private readonly string _tableName;

        public DatabaseSink(
            DBType dataBaseType,
            string connectionString,
            string tableName = "SerLogs",
            bool storeTimestampInUtc = false)
        {
            ASqlManager.DataBaseType = dataBaseType;
            _connectionString = connectionString;
            _tableName = tableName;
            _storeTimestampInUtc = storeTimestampInUtc;

            var sqlConnection = GetSqlConnection();
            string sCheckTable = "";
            sCheckTable = CheckTable(sqlConnection,tableName);
            if(sCheckTable.ToUpper() != tableName.ToUpper())
                CreateTable(sqlConnection);
        }

        public void Emit(LogEvent logEvent)
        {
            WriteLog(logEvent);
        }

        private ASqlConnection GetSqlConnection()
        {
            try
            {
                var conn = new ASqlConnection(_connectionString);
                conn.Open();
                return conn;
            }
            catch (Exception ex)
            {
                SelfLog.WriteLine(ex.Message);
                return null;
            }
        }

        private ASqlCommand GetInsertCommand(ASqlConnection sqlConnection)
        {
            string insertChar = "";
            string insertValues = "";
            switch (ASqlManager.DataBaseType)
            {
                case (DBType.SqlServer):
                    insertChar = "@";
                    insertValues = $"VALUES ({insertChar}SerTs, {insertChar}SerLevel,{insertChar}SerTemplate, {insertChar}SerMsg, {insertChar}SerEx, {insertChar}SerProp)";
                    break;
                case (DBType.Oracle):
                    insertChar = ":";
                    insertValues = $"VALUES ({insertChar}SerTs, {insertChar}SerLevel,{insertChar}SerTemplate, {insertChar}SerMsg, {insertChar}SerEx, {insertChar}SerProp)";
                    break;
                case (DBType.MySql):
                    insertChar = "?";
                    insertValues = $"VALUES ({insertChar}, {insertChar},{insertChar}, {insertChar}, {insertChar}, {insertChar})";
                    break;
                case (DBType.PostgreSQL):
                    insertChar = "@";
                    insertValues = $"VALUES ({insertChar}SerTs, {insertChar}SerLevel,{insertChar}SerTemplate, {insertChar}SerMsg, {insertChar}SerEx, {insertChar}SerProp)";
                    break;
                case (DBType.Sqlite):
                    insertChar = "@";
                    insertValues = $"VALUES ({insertChar}SerTs, {insertChar}SerLevel,{insertChar}SerTemplate, {insertChar}SerMsg, {insertChar}SerEx, {insertChar}SerProp)";
                    break;
                default:
                    throw new NotImplementedException();

            }
            var tableCommandBuilder = new StringBuilder();
            tableCommandBuilder.Append($"INSERT INTO  {_tableName} (");
            tableCommandBuilder.Append("SerTimestamp, SerLevel, SerTemplate, SerMessage, SerException, SerProperties) ");
            tableCommandBuilder.Append(insertValues);
            ASqlCommand cmd = new ASqlCommand(tableCommandBuilder.ToString(), sqlConnection);
            return cmd;
        }
        public static int ExecuteNonQuery(ASqlConnection conn, string sql)
        {
            ASqlCommand cmd = new ASqlCommand(sql, conn);
            return cmd.ExecuteNonQuery();
        }
        public static string CreateSequnce(string tableName)
        {
            string query = $"CREATE SEQUENCE {tableName}_seq START WITH 1 INCREMENT BY 1";
            return query;
        }
        public static string CreateTrigger(string tableName)
        {
            string query = $"CREATE OR REPLACE TRIGGER {tableName}_seq_tr " +
                           $"BEFORE INSERT ON {tableName} FOR EACH ROW " +
                           "WHEN (NEW.id IS NULL) " +
                           "BEGIN " +
                           $"SELECT {tableName}_seq.NEXTVAL INTO :NEW.id FROM DUAL; " +
                           "END; ";
            return query;
        }
        public static string CheckTable(ASqlConnection conn,string TableName)
        {
            string sql = "";
            switch (ASqlManager.DataBaseType)
            {
                case (DBType.SqlServer):
                    sql = sqlCheckTable;
                    break;
                case (DBType.Oracle):
                    sql = oraCheckTable;
                    TableName = TableName.ToUpper();
                    break;
                case (DBType.MySql):
                    sql = mysCheckTable;
                    break;
                case (DBType.PostgreSQL):
                    sql = posCheckTable;
                    break;
                case (DBType.Sqlite):
                    sql = litCheckTable;
                    break;
                default:
                    throw new NotImplementedException();

            }
            sql = sql.Replace("{TableName}", TableName);
            string table = "";
            ASqlCommand cmd = new ASqlCommand(sql, conn);
            switch (conn.DataBaseType)
            {
                case ASqlManager.DBType.Oracle:
                case ASqlManager.DBType.PostgreSQL:
                    using (DbDataReader read = cmd.ExecuteReader())
                    {
                        while (read.Read())
                        {
                            table = read.GetString(read.GetOrdinal("table_name"));
                        }
                    }
                    break;
                case ASqlManager.DBType.SqlServer:
                    bool exists = (int)cmd.ExecuteScalar() == 1;
                    if (exists) { table = TableName; }
                    break;
                case ASqlManager.DBType.MySql:
                    using (DbDataReader read = cmd.ExecuteReader())
                    {
                        while (read.Read())
                        {
                            table = read.GetInt32(read.GetOrdinal("ntab")).ToString();
                            if (table == "1") table = TableName;
                        }
                    }
                    break;
                case ASqlManager.DBType.Sqlite:
                    using (DbDataReader read = cmd.ExecuteReader())
                    {
                        while (read.Read())
                        {
                            table = read.GetString(read.GetOrdinal("name"));
                        }
                    }
                    break;
            }
            return table;
        }
        private int CreateTable(ASqlConnection sqlConnection)
        {
            int res = 0;
            try
            {
                string tableCommandBuilder = "";
                switch (ASqlManager.DataBaseType)
                {
                    case (DBType.SqlServer):
                        tableCommandBuilder = sqlCreateTable.Replace("{TableName}", this._tableName);
                        break;
                    case (DBType.Oracle):
                        tableCommandBuilder = oraCreateTable.Replace("{TableName}", this._tableName);
                        break;
                    case (DBType.MySql):
                        tableCommandBuilder = mysCreateTable.Replace("{TableName}", this._tableName);
                        break;
                    case (DBType.PostgreSQL):
                        tableCommandBuilder = posCreateTable.Replace("{TableName}", this._tableName);
                        break;
                    case (DBType.Sqlite):
                        tableCommandBuilder = litCreateTable.Replace("{TableName}", this._tableName);
                        break;
                    default:
                        throw new NotImplementedException();

                }

                var cmd = sqlConnection.CreateCommand();
                cmd.CommandText = tableCommandBuilder.ToString();
                res = cmd.ExecuteNonQuery();
                if (sqlConnection.DataBaseType == ASqlManager.DBType.Oracle)
                {
                    ExecuteNonQuery(sqlConnection, CreateSequnce(this._tableName));
                    ExecuteNonQuery(sqlConnection, CreateTrigger(this._tableName));
                }
            }
            catch (Exception ex)
            {
                SelfLog.WriteLine(ex.Message);
            }
            return res;
        }

        protected bool WriteLog(LogEvent logEvent)
        {
            try
            {
                using (var sqlCon = GetSqlConnection())
                {
                    ASqlCommand insertCommand = GetInsertCommand(sqlCon);
                    var logMessageString = new StringWriter(new StringBuilder());
                    logEvent.RenderMessage(logMessageString);
                    insertCommand.aSqlParameters.Add(new ASqlParameter { ParameterName = "SerTs", Value = _storeTimestampInUtc ? logEvent.Timestamp.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss.fffzzz") : logEvent.Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fffzzz") });
                    insertCommand.aSqlParameters.Add(new ASqlParameter { ParameterName = "SerLevel", Value = logEvent.Level.ToString() });
                    insertCommand.aSqlParameters.Add(new ASqlParameter { ParameterName = "SerTemplate", Value = logEvent.MessageTemplate.ToString() });
                    insertCommand.aSqlParameters.Add(new ASqlParameter { ParameterName = "SerMsg", Value = logMessageString.ToString() });
                    insertCommand.aSqlParameters.Add(new ASqlParameter { ParameterName = "SerEx", Value = logEvent.Exception==null ? "" : logEvent.Exception?.ToString() });
                    insertCommand.aSqlParameters.Add(new ASqlParameter { ParameterName = "SerProp", Value = logEvent.Properties.Count > 0 ? logEvent.Properties.Json() : string.Empty });
                    insertCommand.ExecuteNonQuery();

                    return true;

                }
            }
            catch (Exception ex)
            {
                SelfLog.WriteLine(ex.Message);

                return false;
            }
        }
    }
}
