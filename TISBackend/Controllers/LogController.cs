using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Web.Http;
using TISBackend.Auth;
using TISBackend.Db;
using TISModelLibrary;

namespace TISBackend.Controllers
{
    public class LogController : ApiController
    {
        // GET: api/Log
        public IEnumerable<LogEntry> Get([FromUri] DateTime? from = null, [FromUri] DateTime? to = null, [FromUri] string table = null, [FromUri] string action = null)
        {
            List<LogEntry> list = new List<LogEntry>();

            if (AuthController.Check(AuthToken.From(Request.Headers)) == AuthLevel.ADMIN)
            {
                List<string> conditions = new List<string>();
                List<OracleParameter> parameters = new List<OracleParameter>();

                if (from != null)
                {
                    conditions.Add("cas >= :from");
                    parameters.Add(new OracleParameter(":from", from.Value));
                }
                if (to != null)
                {
                    conditions.Add("cas <= :to");
                    parameters.Add(new OracleParameter(":to", to.Value));
                }
                if (table != null)
                {
                    conditions.Add("tabulka = :tab");
                    parameters.Add(new OracleParameter(":tab", table.ToLower()));
                }
                if (action != null)
                {
                    conditions.Add("udalost = :action");
                    parameters.Add(new OracleParameter(":action", action.ToLower()));
                }

                StringBuilder queryText = new StringBuilder("SELECT * FROM LOGTABLE");
                if (conditions.Count > 0)
                {
                    queryText.Append(" WHERE");
                    foreach (string condition in conditions)
                    {
                        queryText.Append(" ").Append(condition);
                    }
                }

                DataTable query = DatabaseController.Query(queryText.ToString(), parameters.ToArray());
                foreach (DataRow dr in query.Rows)
                {
                    list.Add(new LogEntry()
                    {
                        Table = dr["tabulka"].ToString(),
                        Event = dr["udalost"].ToString(),
                        Time = DateTime.Parse(dr["cas"].ToString()),
                        Message = dr["zprava"]?.ToString()
                    });
                }
            }

            return list;
        }
    }
}
