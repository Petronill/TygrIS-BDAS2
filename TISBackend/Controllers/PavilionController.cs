using Oracle.ManagedDataAccess.Client;
using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Web.Http;
using TISBackend.Db;
using TISModelLibrary;

namespace TISBackend.Controllers
{
    public class PavilionController : TISController
    {
        private const string tableName = "PAVILONY";
        private const string idName = "id_pavilon";

        private static Pavilion New(DataRow dr, string idName = PavilionController.idName)
        {
            return new Pavilion()
            {
                Id = int.Parse(dr[idName].ToString()),
                Name = dr["nazev"].ToString()
            };
        }

        // GET: api/Pavilion
        public IEnumerable<Pavilion> Get()
        {
            List<Pavilion> list = new List<Pavilion>();

            if (IsAuthorized())
            {
                DataTable query = DatabaseController.Query($"SELECT * FROM {tableName}");
                foreach (DataRow dr in query.Rows)
                {
                    list.Add(New(dr));
                }
            }

            return list;
        }

        // GET: api/Pavilion/5
        public Pavilion Get(int id)
        {
            if (!IsAuthorized())
            {
                return null;
            }
            DataRow query = DatabaseController.Query($"SELECT * FROM {tableName} WHERE {idName} = :id", new OracleParameter("id", id)).Rows[0];
            return New(query);
        }

        // POST: api/Pavilion
        public IHttpActionResult Post([FromBody] string value)
        {
            if (!IsAdmin())
            {
                return StatusCode(HttpStatusCode.Unauthorized);
            }

            // TODO

            return StatusCode(HttpStatusCode.OK);
        }

        // DELETE: api/Pavilion/5
        public IHttpActionResult Delete(int id)
        {
            return DeleteById(tableName, idName, id);
        }
    }
}
