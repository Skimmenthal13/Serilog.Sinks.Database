using ASql;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Configuration;
using static ASql.ASqlManager;

namespace Serilog.Sinks.Database.Tester
{
    [TestClass]
    public class DatabaseLogTester
    {
        [DataRow(DBType.SqlServer, "Data Source=NBK-437;Persist Security Info=True;Initial Catalog=test;Integrated Security=SSPI;")]
        [DataRow(DBType.Oracle, "Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=tcp)(HOST=localhost)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=XEPDB1)));User Id=DBWUSR;Password=DBWUSR;")]
        [DataRow(DBType.MySql, "Server=localhost;Database=test;Uid=sa;Pwd=ASqlAdmin01;")]
        [DataRow(DBType.PostgreSQL, "Server=127.0.0.1;Port=5432;Database=test;User Id=postgres;Password=ASqlAdmin01;")]
        [DataRow(DBType.Sqlite, @"Data Source=c:\temp\test.db;")]
        [TestMethod]
        public void SimpleLogProgrammatically(DBType databaseType,string connString)
        {
            DBType dbType = DBType.MySql;
            
            var logger = new LoggerConfiguration()
                .WriteTo.Database(dbType, "Server=localhost;Database=test;Uid=sa;Pwd=ASqlAdmin01;", "SerLogs",Events.LogEventLevel.Verbose,false,1)
                .CreateLogger();

            logger.Information("This informational message will be written to wich database you want");
        }

        
    }
}