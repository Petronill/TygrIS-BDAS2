using Oracle.ManagedDataAccess.Client;
using System;
using System.Data;

namespace TygrISFramework
{
    internal class Program
    {
        static void Main(string[] args)
        {
            try
            {
                // Please replace the connection string attribute settings
                string constr = "Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=fei-sql1.upceucebny.cz)(PORT=1521)))(CONNECT_DATA=(SID=IDAS)));" +
                    "user id=st64113;password=pgxjm;" +
                    "Connection Timeout=120;Validate connection=true;Min Pool Size=4;";

                OracleConnection con = new OracleConnection(constr);
                con.Open();
                Console.WriteLine("Connected to Oracle Database {0}", con.ServerVersion);

                int id_zvirete = 6;
                string sql = $"SELECT \"ST64113\".\"GET_OSETROVATEL\"({id_zvirete}).jmeno \"jmeno\", \"ST64113\".\"GET_OSETROVATEL\"({id_zvirete}).prijmeni \"prijmeni\" FROM DUAL";
                OracleDataAdapter oda = new OracleDataAdapter(sql, con);
                DataTable dataTable = new DataTable();
                oda.Fill(dataTable);

                foreach (DataRow dr in dataTable.Rows)
                {
                    Console.WriteLine($"{dr["jmeno"]}, {dr["prijmeni"]}");
                }

                con.Dispose();

                Console.WriteLine("Press RETURN to exit.");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error : {0}", ex);

                Console.WriteLine("Press RETURN to exit.");
                Console.ReadLine();
            }
        }
    }
}
