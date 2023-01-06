using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Runtime.Caching;
using System.Web.Http;
using TISBackend.Auth;
using TISBackend.Db;
using TISModelLibrary;

namespace TISBackend.Controllers
{
    public class UserController : TISControllerWithString
    {
        protected static readonly ObjectCache cachedUsers = MemoryCache.Default;

        [Route("api/id/user")]
        public int? GetId()
        {
            AuthToken? authToken = AuthToken.From(Request.Headers);
            if (AuthController.Check(authToken) == AuthLevel.NONE)
            {
                return null;
            }

            DataTable query = DatabaseController.Query($"SELECT id_clovek FROM UCTY jmeno = :id", new OracleParameter("id", authToken.Value.Username));
            
            if (query.Rows.Count != 1)
            {
                return null;
            }            

            return int.Parse((query.Rows[0])["id_clovek"].ToString());
        }

        [Route("api/id/user/{id}")]
        public int? GetId(string id)
        {
            if (IsAdmin())
            {
                return null;
            }

            DataTable query = DatabaseController.Query($"SELECT id_clovek FROM UCTY jmeno = :id", new OracleParameter("id", id));

            if (query.Rows.Count != 1)
            {
                return null;
            }

            return int.Parse((query.Rows[0])["id_clovek"].ToString());
        }

        [Route("api/id/userbyid/{id}")]
        public IEnumerable<string> GetAllIdsById(int id)
        {
            List<string> list = new List<string>();

            if (IsAdmin())
            {
                DataTable query = DatabaseController.Query($"SELECT jmeno FROM UCTY id_clovek = :id", new OracleParameter("id", id));

                foreach (DataRow dr in query.Rows)
                {
                    list.Add(dr["jmeno"].ToString());
                }
            }

            return list;
        }

        // GET: api/User
        public Person Get()
        {
            AuthToken? authToken = AuthToken.From(Request.Headers);
            if (AuthController.Check(authToken) != AuthLevel.NONE)
            {
                DataTable query = DatabaseController.Query($"SELECT t2.*, t3.* FROM UCTY t1 JOIN LIDE t2 ON t1.id_clovek = t2.id_clovek JOIN ADRESY t3 ON t2.id_adresa = t3.id_adresa WHERE t1.jmeno = :id", new OracleParameter("id", authToken.Value.Username));
                if (query.Rows.Count > 0)
                {
                    DataRow dr = query.Rows[0];
                    return PersonController.New(dr, AuthController.Check(authToken));
                }
            }

            return null;
        }

        // GET: api/User/id
        public Person Get(string id)
        {
            if (!IsAdmin())
            {
                return null;
            }
            
            if (cachedUsers[id] is Person)
            {
                return cachedUsers[id] as Person;
            }
                
            DataTable query = DatabaseController.Query($"SELECT t2.*, t3.* FROM UCTY t1 JOIN LIDE t2 ON t1.id_clovek = t2.id_clovek JOIN ADRESY t3 ON t2.id_adresa = t3.id_adresa WHERE t1.jmeno = :id", new OracleParameter("id", id));
            
            if (query.Rows.Count != 1)
            {
                return null;
            }

            Person user = PersonController.New(query.Rows[0], GetAuthLevel());
            cachedUsers.Add(id, user, DateTimeOffset.Now.AddMinutes(15));
            return user;
        }

        // POST: api/User/id
        public IHttpActionResult Post(int id)
        {
            if (!IsAdmin())
            {
                return StatusCode(HttpStatusCode.Forbidden);
            }

            DatabaseController.Execute("PKG_HESLA.NASTAV_CLOVEKA",
                    new OracleParameter("p_jmeno", AuthToken.From(Request.Headers)?.ToString()),
                    new OracleParameter("p_id_clovek", id));
            return StatusCode(HttpStatusCode.OK);
        }

        // DELETE: api/User
        public IHttpActionResult Delete()
        {
            if (!IsAdmin())
            {
                return StatusCode(HttpStatusCode.Forbidden);
            }

            DatabaseController.Execute("PKG_HESLA.NASTAV_CLOVEKA",
                    new OracleParameter("p_jmeno", AuthToken.From(Request.Headers)?.ToString()),
                    new OracleParameter("p_id_clovek", null));
            return StatusCode(HttpStatusCode.OK);
        }

        // DELETE: api/User/id
        public IHttpActionResult Delete(string id)
        {
            if (!IsAdmin())
            {
                return StatusCode(HttpStatusCode.Forbidden);
            }

            DatabaseController.Execute("PKG_HESLA.NASTAV_CLOVEKA",
                    new OracleParameter("p_jmeno", id),
                    new OracleParameter("p_id_clovek", null));
            return StatusCode(HttpStatusCode.OK);
        }
    }
}
