# ASql

C# Library that let use multiple types of databases ASql C# Library that let use multiple types of databases (SqlServer, Oracle, MySql, PostgreSQL, Sqlite) with same code with same code

The supported databases are SqlServer, Oracle, MySql, PostgreSQL and Sqlite

Here you can find last stable from Nuget www.nuget.org/packages/ASql

You only have to declare wich type of database would like you use and use the classe in the same way of sql or oracle client.

Inside our library We use the most famous libraries for connecting to these 5 dabases :

| Database   | Library                    | Example of connection string                                                                                                                                                            |
| ---------- | -------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| SqlServer  | System.Data.SqlClient      | const string sqlConnectionString = "Data Source=NBK-437;Persist Security Info=True;Initial Catalog=test;Integrated Security=SSPI;";                                                     |
| Oracle     | Oracle.ManagedDataAccess   | const string oraConnectionString = "Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=tcp)(HOST=localhost)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=XEPDB1)));User Id=DBWUSR;Password=DBWUSR;"; |
| MySql      | MySql.Data                 | const string mysConnectionString = "Server=localhost;Database=test;Uid=sa;Pwd=ASqlAdmin01;";                                                                                            |
| PostgreSQL | Npgsql                     | const string posConnectionString = "Server=127.0.0.1;Port=5432;Database=test;User Id=postgres;Password=ASqlAdmin01;";                                                                   |
| Sqlite     | Microsoft.Data.Sqlite      | const string litConnectionString = @"Data Source=c:\temp\test.db;";                                                                                                                     |

We have added the event OnGenericQueryEnd to trace all the query and their performances executed in your software.

We have added the MultiDatabase option and you can use more connections at the same time (useful if you have to implement migration tools or you have data to handle in multiple databases inside your software)

Here you can find quick examples but if you want more we suggest to download the GitHub project and watch the ASql.Tester Project : 

            # Example 1 Basic query

            public void ExecuteQuery(ASqlManager.DBType dBType, string ConnectionString, string sql)
            {
                //You only have to specify which type of database want to use and after you will have to use ASql objects instead the Typized objects like OracleConnection
                ASqlManager.DataBaseType = dBType;

                using (ASqlConnection conn = new ASqlConnection(ConnectionString))
                {
                    int i = 0;
                    conn.Open();
                    ASqlCommand cmd = new ASqlCommand(sql, conn);
                    i = cmd.ExecuteNonQuery();
                }
            }

            # Example 2.a Query with ExecuteScalar With Parameters

            public void ExecuteScalar(ASqlManager.DBType dBType, string ConnectionString)
            {
                string sql = "select firstname from person where lastname = @lastname";
                //if you want to use oracle you have tu put : intestad @ like this 
                //string sql = "select firstname from person where lastname = :lastname";
                ASqlManager.DataBaseType = dBType;

                using (ASqlConnection conn = new ASqlConnection(ConnectionString))
                {
                    string name = "";
                    conn.Open();
                    ASqlCommand cmd = new ASqlCommand(sql, conn);
                    ASqlParameter par = new ASqlParameter();
                    par.ParameterName = "lastname";
                    par.DbType = DbType.String;
                    par.Value = "last1";
                    //Pay attention to popolate the aSqlParameters collection like the row below and not the Parameters collection (the library will think to keep uptodated the Parameters collection)
                    cmd.aSqlParameters.Add(par);
                    name = (string)cmd.ExecuteScalar();
                }
            }

            # Example 2.b with ExecuteReader With Parameters

            public void ExecuteReaderTester(ASqlManager.DBType dBType)
            {
                string sql = "select firstname from person where lastname = @lastname";
                //if you want to use oracle you have tu put : intestad @ like this 
                //string sql = "select firstname from person where lastname = :lastname";
                ASqlManager.DataBaseType = dBType;

                using (ASqlConnection conn = new ASqlConnection(ConnectionString))
                {
                    string i = "";
                    conn.Open();
                    ASqlCommand cmd = new ASqlCommand(sql, conn);
                    ASqlParameter par = new ASqlParameter();
                    par.ParameterName = "lastname";
                    par.DbType = DbType.String;
                    par.Value = "last1";
                    cmd.aSqlParameters.Add(par);
                    using (DbDataReader read = cmd.ExecuteReader())
                    {
                        while (read.Read())
                        {
                            i = read.GetString(read.GetOrdinal("firstname"));
                        }
                    }

                    Assert.AreEqual("first1", i.ToUpper());
                }
            }

            # Example 3 Using DataAdapter

            public void SqlDataAdapterTester(ASqlManager.DBType dBType)
            {
                ASqlManager.DataBaseType = dBType;
                string sql = "select * from person"
                using (ASqlConnection conn = new ASqlConnection(ConnectionString))
                {
                    int i = 0;
                    conn.Open();
                    ASqlCommand cmd = new ASqlCommand(sql, conn);
                    ASqlDataAdapter aSqlDataAdapter = new ASqlDataAdapter(cmd);
                    DataSet ds = new DataSet();
                    i = aSqlDataAdapter.Fill(ds);

                }
            }

            # Example 4 using Transaction

            public void DropTableWithRollBack(ASqlManager.DBType dBType, string ConnectionString, string sql)
            {
                ASqlManager.DataBaseType = dBType;

                using (ASqlConnection conn = new ASqlConnection(ConnectionString))
                {
                    int i = 0;
                    conn.Open();
                    DbTransaction trans = conn.BeginTransaction();
                    ASqlCommand cmd = new ASqlCommand(sql, conn);
                    cmd.Transaction = trans;
                    i = cmd.ExecuteNonQuery();
                    trans.Rollback();
                }
            }

            # Example 5 using Event to trace queries

            //This method will be called whene ExecuteNonQuery, ExecuteReader and ExcutedScalar will be executed
            //If you want you can use your favourite logging library to trace the queries and their performance in millisecs.
            internal static void Cmd_OnGenericQueryEnd(object sender, GenericQueryEndEventArgs e)
            {
                Console.WriteLine(e.Method+" "+e.Query +" "+ e.TotalMilliseconds);
            }
            
            ASqlManager.DataBaseType = dBType;
            using (ASqlConnection conn = new ASqlConnection(ConnectionString))
            {
                conn.Open();
                ASqlCommand cmd = new ASqlCommand(sql, conn);
                cmd.OnGenericQueryEnd += Cmd_OnGenericQueryEnd; //This is the way to link the method to the event inside the command object
                string paramChar = "";

                List<ASqlParameter> apc = Utils.GetParametersFromkeyValuePairs(d, paramChar);
                foreach (ASqlParameter param in apc)
                {
                    cmd.aSqlParameters.Add(param);
                }

                r = cmd.ExecuteNonQuery();
            }

            # Example 6 using multiple connections at the same time

            ASqlManager.DataBaseType = ASqlManager.DBType.MultiDatabase; //Selecting MultiDatapase enum
            ASqlConnection sqlConn = new ASqlConnection(ASqlManager.DBType.SqlServer, Utils.sqlConnectionString); //Passing DataBase type in the constructor
            ASqlConnection oraConn = new ASqlConnection(ASqlManager.DBType.Oracle, Utils.oraConnectionString);
            sqlConn.Open();
            oraConn.Open();
            DbTransaction sqlTrans = sqlConn.BeginTransaction();
            DbTransaction oraTrans = oraConn.BeginTransaction();
            Utils.InsertRow(sqlConn, Utils.sqlInsertRow, sqlTrans);
            Utils.InsertRow(oraConn, Utils.oraInsertRow, oraTrans);
            sqlTrans.Rollback();
            oraTrans.Rollback();
            sqlConn.Close();
            oraConn.Close();

            public static int InsertRow(ASqlConnection conn, string sql, DbTransaction trans=null) 
            {
                int r = 0;
                ASqlCommand cmd = new ASqlCommand(sql, conn);
                if(trans!=null)
                    cmd.Transaction = trans;
                string paramChar = "";
                List<ASqlParameter> apc = Utils.GetASqlParametersFromScratch(conn.DataBaseType,paramChar); //Passing DataBase type of your connection in the constructor
                foreach (ASqlParameter param in apc)
                {
                    cmd.aSqlParameters.Add(param);
                }
                r = cmd.ExecuteNonQuery();
                return r;
            }