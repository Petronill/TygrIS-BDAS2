using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Configuration;
using System.Web;
using System.Web.Http.Results;

namespace TISBackend.Db
{
    class DatabaseController
    {
        private static string CONSTR =
            "Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=fei-sql3.upceucebny.cz)(PORT=1521)))(CONNECT_DATA=(SID=BDAS)));" +
            "user id=st64113;password=pgxjm;" +
            "Connection Timeout=120;Validate connection=true;Min Pool Size=4;";
        private static DatabaseController _databaseController = new DatabaseController();

        private readonly OracleConnection con = new OracleConnection(CONSTR);

        private DatabaseController()
        {
            con.Open();
        }

        public static DataTable Query(string sql, params OracleParameter[] parameters)
        {
            DataTable dataTable = new DataTable();
            OracleCommand cmd = new OracleCommand(sql, _databaseController.con);
            foreach (OracleParameter op in parameters)
            {
                cmd.Parameters.Add(op);
            }

            OracleDataAdapter oda = new OracleDataAdapter(cmd);
            oda.Fill(dataTable);
            return dataTable;
        }

        public static DataTable Query(string sql, OracleTransaction transaction, params OracleParameter[] parameters)
        {
            DataTable dataTable = new DataTable();
            OracleCommand cmd = new OracleCommand(sql, _databaseController.con);
            if (transaction != null)
            {
                cmd.Transaction = transaction;
            }
            foreach (OracleParameter op in parameters)
            {
                cmd.Parameters.Add(op);
            }

            OracleDataAdapter oda = new OracleDataAdapter(cmd);
            oda.Fill(dataTable);
            return dataTable;
        }

        public static OracleDataReader Read(string sql, params OracleParameter[] parameters)
        {
            OracleCommand cmd = new OracleCommand(sql, _databaseController.con);
            foreach (OracleParameter op in parameters)
            {
                cmd.Parameters.Add(op);
            }

            OracleDataReader reader = cmd.ExecuteReader();
            return reader;
        }

        public static OracleDataReader Read(string sql, OracleTransaction transaction, params OracleParameter[] parameters)
        {
            OracleCommand cmd = new OracleCommand(sql, _databaseController.con);
            if (transaction != null)
            {
                cmd.Transaction = transaction;
            }
            foreach (OracleParameter op in parameters)
            {
                cmd.Parameters.Add(op);
            }

            OracleDataReader reader = cmd.ExecuteReader();
            return reader;
        }

        public static void Execute(string sql, params OracleParameter[] parameters)
        {
            OracleCommand cmd = new OracleCommand(sql, _databaseController.con);
            foreach (OracleParameter op in parameters)
            {
                cmd.Parameters.Add(op);
            }
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.ExecuteNonQuery();
        }

        public static void Execute(string sql, OracleTransaction transaction, params OracleParameter[] parameters)
        {
            OracleCommand cmd = new OracleCommand(sql, _databaseController.con);
            if (transaction != null)
            {
                cmd.Transaction = transaction;
            }
            foreach (OracleParameter op in parameters)
            {
                cmd.Parameters.Add(op);
            }
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.ExecuteNonQuery();
        }

        public static OracleTransaction StartTransaction()
        {
            return _databaseController.con.BeginTransaction();
        }

        public static void Commit(OracleTransaction transaction)
        {
            transaction.Commit();
        }

        public static void Rollback(OracleTransaction transaction)
        {
            transaction.Rollback();
        }

        ~DatabaseController()
        {
            con.Dispose();
        }
    }
}