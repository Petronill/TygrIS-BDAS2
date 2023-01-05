using Newtonsoft.Json.Linq;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System.Collections.Generic;
using System.Web.Http;
using TISBackend.Auth;
using TISBackend.Db;
using TISModelLibrary;

namespace TISBackend.Controllers
{
    public class FileController : TISControllerWithInt
    {
        public const string TABLE_NAME = "DOKUMENTY";
        public const string ID_NAME = "id_dokument";

        private static readonly FileController instance = new FileController();

        [Route("api/id/file")]
        public IEnumerable<int> GetIds()
        {
            return GetIds(TABLE_NAME, ID_NAME);
        }

        [NonAction]
        public static Document New(OracleDataReader reader, AuthLevel authLevel)
        {
            Document document = null;
            if (reader.Read())
            {
                document = new Document
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Extension = reader.GetString(2)
                };
                OracleBlob blob = reader.GetOracleBlob(3);
                byte[] blobBytes = new byte[blob.Length];
                blob.Read(blobBytes, 0, blobBytes.Length);
                document.Data = Document.SerializeBytes(blobBytes);
            }
            reader.Close();

            return document;
        }

        // GET: api/File/5
        public Document Get(int id)
        {
            if (!IsAuthorized())
            {
                return null;
            }

            OracleDataReader reader = DatabaseController.Read($"SELECT {ID_NAME}, nazev_souboru, pripona, dokument FROM {TABLE_NAME} WHERE {ID_NAME} = :id", new OracleParameter("id", id));
            return New(reader, GetAuthLevel());
        }

        [NonAction]
        protected override bool CheckObject(JObject value, AuthLevel authLevel)
        {
            return ValidJSON(value, "Id", "Name", "Extension", "Data") && int.TryParse(value["Id"].ToString(), out _);
        }

        [NonAction]
        protected override int SetObjectInternal(JObject value, AuthLevel authLevel, OracleTransaction transaction)
        {
            Document n = value.ToObject<Document>();
            OracleParameter p_id = new OracleParameter("p_id", n.Id);
            OracleParameter p_data = new OracleParameter("p_data", OracleDbType.Blob)
            {
                Value = Document.DeserializeBytes(n.Data)
            };
            DatabaseController.Execute("PKG_MODEL_DML.UPSERT_DOKUMENT", transaction,
                p_id,
                new OracleParameter("p_nazev", n.Name),
                new OracleParameter("p_pripona", n.Extension),
                p_data);
            
            return int.Parse(p_id.Value.ToString());
        }

        [NonAction]
        public static bool CheckObjectStatic(JObject value, AuthLevel authLevel)
        {
            return instance.CheckObject(value, authLevel);
        }

        [NonAction]
        public static int SetObjectStatic(JObject value, AuthLevel authLevel, OracleTransaction transaction = null)
        {
            return instance.SetObject(value, authLevel, transaction);
        }

        // POST: api/Document
        public IHttpActionResult Post([FromBody] JObject value)
        {
            return PostUnknownNumber(value);
        }

        // POST: api/Document/5
        public IHttpActionResult Post(int id, [FromBody] JObject value)
        {
            return PostSingle(id, value);
        }

        // DELETE: api/File/5
        public IHttpActionResult Delete(int id)
        {
            return DeleteById(TABLE_NAME, ID_NAME, id);
        }

    }
}
