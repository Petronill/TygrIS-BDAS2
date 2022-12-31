using Oracle.ManagedDataAccess.Client;
using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Web.Http;
using TISBackend.Db;
using TISBackend.Models;

namespace TISBackend.Controllers
{
    public class GenusController : TISController
    {
        private const string tableName = "RODY";
        private const string idName = "id_rod";

        // GET: api/Genus
        public IEnumerable<Genus> Get()
        {
            List<Genus> list = new List<Genus>();

            if (IsAuthorized())
            {
                DataTable query = DatabaseController.Query($"SELECT * FROM {tableName}");
                foreach (DataRow dr in query.Rows)
                {
                    list.Add(new Genus()
                    {
                        Id = int.Parse(dr[idName].ToString()),
                        CzechName = dr["jmeno_rodu_cesky"].ToString(),
                        LatinName = dr["jmeno_rodu_latinsky"].ToString()
                    });
                }
            }

            return list;
        }

        // GET: api/Genus/5
        public Genus Get(int id)
        {
            if (!IsAuthorized())
            {
                return null;
            }
            DataRow query = DatabaseController.Query($"SELECT * FROM {tableName} WHERE {idName} = :id", new OracleParameter("id", id)).Rows[0];
            return new Genus()
            {
                Id = int.Parse(query[idName].ToString()),
                CzechName = query["jmeno_rodu_cesky"].ToString(),
                LatinName = query["jmeno_rodu_latinsky"].ToString()
            };
        }

        // POST: api/Genus
        public IHttpActionResult Post([FromBody] string value)
        {
            if (!IsAdmin())
            {
                return StatusCode(HttpStatusCode.Unauthorized);
            }

            // TODO

            return StatusCode(HttpStatusCode.OK);
        }

        // DELETE: api/Genus/5
        public IHttpActionResult Delete(int id)
        {
            return DeleteById(tableName, idName, id);
        }
    }
}
