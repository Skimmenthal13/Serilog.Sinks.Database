# Serilog.Sinks.Database
Serilog sink that writes in one of these five databases 
| Database   | Library                    | Example of connection string                                                                                                                                                            |
| ---------- | -------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| SqlServer  | System.Data.SqlClient      | const string sqlConnectionString = "Data Source=NBK-437;Persist Security Info=True;Initial Catalog=test;Integrated Security=SSPI;";                                                     |
| Oracle     | Oracle.ManagedDataAccess   | const string oraConnectionString = "Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=tcp)(HOST=localhost)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=XEPDB1)));User Id=DBWUSR;Password=DBWUSR;"; |
| MySql      | MySql.Data                 | const string mysConnectionString = "Server=localhost;Database=test;Uid=sa;Pwd=ASqlAdmin01;";                                                                                            |
| PostgreSQL | Npgsql                     | const string posConnectionString = "Server=127.0.0.1;Port=5432;Database=test;User Id=postgres;Password=ASqlAdmin01;";                                                                   |
| Sqlite     | Microsoft.Data.Sqlite      | const string litConnectionString = @"Data Source=c:\temp\test.db;";                                                                                                                     |

## Getting started

Install [Serilog.Sinks.Database](https://www.nuget.org/packages/Serilog.Sinks.Database) from NuGet

```PowerShell
Install-Package Serilog.Sinks.Database
```

Configure logger by calling WriteTo.Database

```C#
DBType dbType = DBType.MySql;
            
var logger = new LoggerConfiguration()
      .WriteTo.Database(dbType, "Server=localhost;Database=test;Uid=sa;Pwd=ASqlAdmin01;", "SerLogs",Events.LogEventLevel.Verbose,false,1)
      .CreateLogger();

logger.Information("This informational message will be written to wich database you want");
```