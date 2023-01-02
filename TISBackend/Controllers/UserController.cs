using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
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
        public IEnumerable<int> GetIds()
        {
            List<int> list = new List<int>();

            AuthToken? authToken = AuthToken.From(Request.Headers);
            if (AuthController.Check(authToken) != AuthLevel.NONE)
            {
                DataTable query = DatabaseController.Query($"SELECT t2.id_clovek FROM UCTY t1 JOIN LIDE t2 ON t1.id_clovek = t2.id_clovek WHERE t1.jmeno = :id", new OracleParameter("id", authToken.Value.Username));
                foreach (DataRow dr in query.Rows)
                {
                    list.Add(int.Parse(dr["id_clovek"].ToString()));
                }
            }

            return list;
        }

        [Route("api/id/user/{id}")]
        public IEnumerable<int> GetIds(string id)
        {
            List<int> list = new List<int>();

            if (IsAdmin())
            {
                DataTable query = DatabaseController.Query($"SELECT t2.id_clovek FROM UCTY t1 JOIN LIDE t2 ON t1.id_clovek = t2.id_clovek WHERE t1.jmeno = :id", new OracleParameter("id", id));
                foreach (DataRow dr in query.Rows)
                {
                    list.Add(int.Parse(dr["id_clovek"].ToString()));
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
    }
}
