using Newtonsoft.Json.Linq;
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
    public class TitleController : TISControllerWithInt
    {
        public const string TABLE_NAME = "TITULY";
        public const string ID_NAME = "id_titul";

        protected static readonly ObjectCache cachedTitles = MemoryCache.Default;

        private static readonly TitleController instance = new TitleController();

        [NonAction]
        public static Title New(DataRow dr, AuthLevel authLevel, string idName = TitleController.ID_NAME)
        {
            return new Title()
            {
                Id = int.Parse(dr[idName].ToString()),
                TitleName = dr["nazev"].ToString(),
                Abbreviation = dr["zkratka"].ToString()
            };
        }

        [Route("api/id/title")]
        public IEnumerable<int> GetIds()
        {
            return GetIds(TABLE_NAME, ID_NAME);
        }

        // GET: api/Title
        public IEnumerable<Title> Get()
        {
            List<Title> list = new List<Title>();

            if (IsAuthorized())
            {
                DataTable query = DatabaseController.Query($"SELECT * FROM {TABLE_NAME}");
                foreach (DataRow dr in query.Rows)
                {
                    list.Add(New(dr, GetAuthLevel()));
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


            if (cachedTitles[id.ToString()] is Title)
            {
                return cachedTitles[id.ToString()] as Title;
            }

            DataTable query = DatabaseController.Query($"SELECT * FROM {TABLE_NAME} WHERE {ID_NAME} = :id", new OracleParameter("id", id));

            if (query.Rows.Count != 1)
            {
                return null;
            }

            Title title = New(query.Rows[0], GetAuthLevel());
            cachedTitles.Add(id.ToString(), title, DateTimeOffset.Now.AddMinutes(15));
            return title;
        }

        [NonAction]
        protected override bool CheckObject(JObject value)
        {
            return ValidJSON(value, "Id", "TitleName", "Abbreviation") && int.TryParse(value["Id"].ToString(), out _);
        }

        [NonAction]
        protected override int SetObjectInternal(JObject value, AuthLevel authLevel, OracleTransaction transaction)
        {
            Title n = value.ToObject<Title>();
            OracleParameter p_id = new OracleParameter("p_id", n.Id);
            DatabaseController.Execute("PKG_MODEL_DML.UPSERT_TITUL", transaction,
                p_id,
                new OracleParameter("p_nazev", n.TitleName),
                new OracleParameter("p_zkratka", n.Abbreviation));
            int id = int.Parse(p_id.Value.ToString());

            if (id != ErrId && cachedTitles.Contains(id.ToString()))
            {
                n.Id = id;
                cachedTitles[id.ToString()] = n;
            }

            return id;
        }

        [NonAction]
        public static bool CheckObjectStatic(JObject value)
        {
            return instance.CheckObject(value);
        }

        [NonAction]
        public static int SetObjectStatic(JObject value, AuthLevel authLevel, OracleTransaction transaction = null)
        {
            return instance.SetObject(value, authLevel, transaction);
        }

        // POST: api/Title
        public IHttpActionResult Post([FromBody] JObject value)
        {
            return PostUnknownNumber(value);
        }

        // POST : api/Title/5
        public IHttpActionResult Post(int id, [FromBody] JObject value)
        {
            return PostSingle(id, value);
        }

        // DELETE: api/Title/5
        public IHttpActionResult Delete(int id)
        {
            return DeleteById(TABLE_NAME, ID_NAME, id, cachedTitles);
        }
    }
}
