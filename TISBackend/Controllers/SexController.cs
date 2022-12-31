using Oracle.ManagedDataAccess.Client;
using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Web.Http;
using TISBackend.Db;
using TISBackend.Models;

namespace TISBackend.Controllers
{
    public class SexController : TISController
    {
        private const string tableName = "POHLAVI";
        private const string idName = "id_pohlavi";

        // GET: api/Sex
        public IEnumerable<Sex> Get()
        {
            List<Sex> list = new List<Sex>();

            if (IsAuthorized())
            {
                DataTable query = DatabaseController.Query($"SELECT * FROM {tableName}");
                foreach (DataRow dr in query.Rows)
                {
                    list.Add(new Sex() {
                        Id = int.Parse(dr[idName].ToString()),
                        Abbreviation = dr["zkratka"].ToString()
                    });
                }
            }

            return list;
        }

        // GET: api/Sex/5
        public Sex Get(int id)
        {
            if (!IsAuthorized())
            {
                return null;
            }
            DataRow query = DatabaseController.Query($"SELECT * FROM {tableName} WHERE {idName} = id", new OracleParameter(":id", id)).Rows[0];
            return new Sex() {
                Id = int.Parse(query[idName].ToString()),
                Abbreviation = query["zkratka"].ToString()
            };
        }

        // POST: api/Sex
        public IHttpActionResult Post([FromBody]string value)
        {
            if (!IsAdmin())
            {
                return StatusCode(HttpStatusCode.Unauthorized);
            }

            // TODO

            return StatusCode(HttpStatusCode.OK);
        }

        // DELETE: api/Sex/5
        public IHttpActionResult Delete(int id)
        {
            return DeleteById(tableName, idName, id);
        }
    }
}
