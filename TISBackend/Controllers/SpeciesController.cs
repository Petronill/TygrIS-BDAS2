using Oracle.ManagedDataAccess.Client;
using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Web.Http;
using TISBackend.Db;
using TISBackend.Models;

namespace TISBackend.Controllers
{
    public class SpeciesController : TISController
    {
        private const string tableName = "DRUHY";
        private const string idName = "id_druh";
        private const string superTableName = "RODY";
        private const string superIdName = "id_rod";

        // GET: api/Species
        public IEnumerable<Species> Get()
        {
            List<Species> list = new List<Species>();

            if (IsAuthorized())
            {
                DataTable query = DatabaseController.Query($"SELECT * FROM {tableName} JOIN {superTableName} USING ({superIdName})");
                foreach (DataRow dr in query.Rows)
                {
                    list.Add(new Species()
                    {
                        Id = int.Parse(dr[idName].ToString()),
                        CzechName = dr["jmeno_druhu_cesky"].ToString(),
                        LatinName = dr["jmeno_druhu_latinsky"].ToString(),
                        Genus = new Genus()
                        {
                            Id = int.Parse(dr["id_rod"].ToString()),
                            CzechName = dr["jmeno_rodu_cesky"].ToString(),
                            LatinName = dr["jmeno_rodu_latinsky"].ToString()
                        }
                    });
                }
            }

            return list;
        }

        // GET: api/Species/5
        public Species Get(int id)
        {
            if (!IsAuthorized())
            {
                return null;
            }
            DataRow query = DatabaseController.Query($"SELECT * FROM {tableName} JOIN {superTableName} USING ({superIdName}) WHERE {idName} = :id", new OracleParameter("id", id)).Rows[0];
            return new Species()
            {
                Id = int.Parse(query[idName].ToString()),
                CzechName = query["jmeno_druhu_cesky"].ToString(),
                LatinName = query["jmeno_druhu_latinsky"].ToString(),
                Genus = new Genus()
                {
                    Id = int.Parse(query["id_rod"].ToString()),
                    CzechName = query["jmeno_rodu_cesky"].ToString(),
                    LatinName = query["jmeno_rodu_latinsky"].ToString()
                }
            };
        }

        // POST: api/Species
        public IHttpActionResult Post([FromBody] string value)
        {
            if (!IsAdmin())
            {
                return StatusCode(HttpStatusCode.Unauthorized);
            }

            // TODO

            return StatusCode(HttpStatusCode.OK);
        }

        // DELETE: api/Species/5
        public IHttpActionResult Delete(int id)
        {
            return DeleteById(tableName, idName, id);
        }
    }
}
