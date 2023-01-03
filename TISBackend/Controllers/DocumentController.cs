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
    public class DocumentController : TISControllerWithInt
    {
        public const string TABLE_NAME = "DOKUMENTY";
        public const string ID_NAME = "id_dokument";

        protected static readonly ObjectCache cachedDocuments = MemoryCache.Default;

        private static readonly DocumentController instance = new DocumentController();

        [NonAction]
        public static Document New(DataRow dr, AuthLevel authLevel, string idName = DocumentController.ID_NAME)
        {
            return new Document()
            {
                Id = int.Parse(dr[idName].ToString()),
                Name = dr["nazev_souboru"].ToString(),
                Extension = dr["pripona"].ToString()
            };
        }

        [Route("api/id/document")]
        public IEnumerable<int> GetIds()
        {
            return GetIds(TABLE_NAME, ID_NAME);
        }

        // GET: api/Document
        public IEnumerable<Document> Get()
        {
            List<Document> list = new List<Document>();

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

        // GET: api/Document/5
        public Document Get(int id)
        {
            if (!IsAuthorized())
            {
                return null;
            }

            if (cachedDocuments[id.ToString()] is Document)
            {
                return cachedDocuments[id.ToString()] as Document;
            }

            DataTable query = DatabaseController.Query($"SELECT * FROM {TABLE_NAME} WHERE {ID_NAME} = :id", new OracleParameter("id", id));

            if (query.Rows.Count != 1)
            {
                return null;
            }

            Document document = New(query.Rows[0], GetAuthLevel());
            cachedDocuments.Add(id.ToString(), document, DateTimeOffset.Now.AddMinutes(15));
            return document;
        }

        // DELETE: api/Document/5
        public IHttpActionResult Delete(int id)
        {
            return DeleteById(TABLE_NAME, ID_NAME, id, cachedDocuments);
        }
    }
}
