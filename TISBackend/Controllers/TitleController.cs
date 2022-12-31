using Oracle.ManagedDataAccess.Client;
using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Web.Http;
using TISBackend.Db;
using TISModelLibrary;

namespace TISBackend.Controllers
{
    public class TitleController : TISController
    {
        private const string tableName = "TITULY";
        private const string idName = "id_titul";

        public static Title New(DataRow dr, string idName = TitleController.idName)
        {
            return new Title()
            {
                Id = int.Parse(dr[idName].ToString()),
                TitleName = dr["nazev"].ToString(),
                Abbreviation = dr["zkratka"].ToString()
            };
        }

        // GET: api/Title
        public IEnumerable<Title> Get()
        {
            List<Title> list = new List<Title>();

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

        // GET: api/Title/5
        public Title Get(int id)
        {
            if (!IsAuthorized())
            {
                return null;
            }
            DataRow query = DatabaseController.Query($"SELECT * FROM {tableName} WHERE {idName} = :id", new OracleParameter("id", id)).Rows[0];
            return New(query);
        }

        // POST: api/Title
        public IHttpActionResult Post([FromBody] string value)
        {
            if (!IsAdmin())
            {
                return StatusCode(HttpStatusCode.Unauthorized);
            }

            // TODO

            return StatusCode(HttpStatusCode.OK);
        }

        // DELETE: api/Title/5
        public IHttpActionResult Delete(int id)
        {
            return DeleteById(tableName, idName, id);
        }
    }
}
