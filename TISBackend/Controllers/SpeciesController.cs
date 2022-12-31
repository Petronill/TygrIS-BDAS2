using Oracle.ManagedDataAccess.Client;
using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Web.Http;
using TISBackend.Db;
using TISModelLibrary;

namespace TISBackend.Controllers
{
    public class SpeciesController : TISController
    {
        private const string tableName = "DRUHY";
        private const string idName = "id_druh";
        private const string superTableName = "RODY";
        private const string superIdName = "id_rod";

        public static Species New(DataRow dr, string idName = SpeciesController.idName)
        {
            return new Species()
            {
                Id = int.Parse(dr[idName].ToString()),
                CzechName = dr["jmeno_druhu_cesky"].ToString(),
                LatinName = dr["jmeno_druhu_latinsky"].ToString(),
                Genus = GenusController.New(dr)
            };
        }

        // GET: api/Species
        public IEnumerable<Species> Get()
        {
            List<Species> list = new List<Species>();

            if (IsAuthorized())
            {
                DataTable query = DatabaseController.Query($"SELECT * FROM {tableName} JOIN {superTableName} USING ({superIdName})");
                foreach (DataRow dr in query.Rows)
                {
                    list.Add(New(dr));
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
            return New(query);
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
