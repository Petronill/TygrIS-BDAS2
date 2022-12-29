using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace TISBackend.Db
{
    class DatabaseController
    {
        private static string CONSTR =
            "Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=fei-sql1.upceucebny.cz)(PORT=1521)))(CONNECT_DATA=(SID=IDAS)));" +
            "user id=st64113;password=pgxjm;" +
            "Connection Timeout=120;Validate connection=true;Min Pool Size=4;";
        private static DatabaseController _databaseController = new DatabaseController();

        private readonly OracleConnection con = new OracleConnection(CONSTR);

        private DatabaseController()
        {
            con.Open();
        }

        public static DataTable Query(string sql)
        {
            DataTable dataTable = new DataTable();
            OracleDataAdapter oda = new OracleDataAdapter(sql, _databaseController.con);
            oda.Fill(dataTable);
            return dataTable;
        }

        ~DatabaseController()
        {
            con.Dispose();
        }
    }
}