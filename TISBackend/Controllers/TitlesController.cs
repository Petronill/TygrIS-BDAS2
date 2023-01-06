using Newtonsoft.Json.Linq;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Security.Cryptography;
using System.Web.Http;
using TISBackend.Auth;
using TISBackend.Db;

namespace TISBackend.Controllers
{
    public class TitlesController : TISControllerWithInt
    {
        public const string TABLE_NAME = "TITULY_LIDI";
        public const string ID_PERSON_NAME = "id_clovek";
        public const string ID_TITLE_NAME = "id_titul";

        private static readonly TitlesController instance = new TitlesController();

        [Route("api/titles/bytitle/{id}")]
        public IEnumerable<int> GetPeople(int id)
        {
            List<int> list = new List<int>();

            if (IsAuthorized())
            {
                DataTable query = DatabaseController.Query($"SELECT {ID_PERSON_NAME} FROM UCTY {ID_TITLE_NAME} = :id", new OracleParameter("id", id));

                foreach (DataRow dr in query.Rows)
                {
                    list.Add(int.Parse(dr[ID_PERSON_NAME].ToString()));
                }
            }

            return list;
        }

        [Route("api/titles/byperson/{id}")]
        public IEnumerable<int> GetTitles(int id)
        {
            List<int> list = new List<int>();

            if (IsAuthorized())
            {
                DataTable query = DatabaseController.Query($"SELECT {ID_TITLE_NAME} FROM UCTY {ID_PERSON_NAME} = :id", new OracleParameter("id", id));

                foreach (DataRow dr in query.Rows)
                {
                    list.Add(int.Parse(dr[ID_TITLE_NAME].ToString()));
                }
            }

            return list;
        }

        [NonAction]
        protected override bool CheckObject(JObject value, AuthLevel authLevel)
        {
            return ValidJSON(value, "TitleId", "PersonId")
                && int.TryParse(value["TitleId"].ToString(), out _)
                && int.TryParse(value["PersonId"].ToString(), out _);
        }

        [NonAction]
        protected new void SetObject(JObject value, AuthLevel authLevel, OracleTransaction transaction = null)
        {
            bool newTransaction = transaction == null;
            transaction = transaction ?? DatabaseController.StartTransaction();

            try
            {
                DatabaseController.Execute("PKG_MODEL_DML.INSERT_TITUL_CLOVEKA", transaction,
                    new OracleParameter("p_id_titul", int.Parse(value["TitleId"].ToString())),
                    new OracleParameter("p_id_clovek", int.Parse(value["PersonId"].ToString())));
            }
            catch (Exception)
            {
                // TODO - logging?
                if (newTransaction) DatabaseController.Rollback(transaction);
#if DEBUG
                throw;
#endif
            }

            if (newTransaction)
            {
                DatabaseController.Commit(transaction);
            }
        }

        [NonAction]
        public static bool CheckObjectStatic(JObject value, AuthLevel authLevel)
        {
            return instance.CheckObject(value, authLevel);
        }

        [NonAction]
        public static void SetObjectStatic(JObject value, AuthLevel authLevel, OracleTransaction transaction = null)
        {
            instance.SetObject(value, authLevel, transaction);
        }

        [NonAction]
        public new IHttpActionResult PostMultiple(JArray array)
        {
            try
            {
                AuthLevel authLevel = GetAuthLevel();
                foreach (JToken token in array)
                {
                    if (token.Type != JTokenType.Object)
                    {
                        return StatusCode(HttpStatusCode.BadRequest);
                    }

                    JObject obj = token.ToObject<JObject>();
                    if (!CheckObject(obj, authLevel))
                    {
                        return StatusCode(HttpStatusCode.BadRequest);
                    }

                    SetObject(obj, authLevel);
                }
                return StatusCode(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.InternalServerError, ex.ToString());
            }
        }

        [NonAction]
        public new IHttpActionResult PostSingle(JObject value)
        {
            try
            {
                AuthLevel authLevel = GetAuthLevel();
                if (!CheckObject(value, authLevel))
                {
                    return StatusCode(HttpStatusCode.BadRequest);
                }

                SetObject(value, authLevel);
                return StatusCode(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.InternalServerError, ex.ToString());
            }
        }

        public IHttpActionResult Post([FromBody] JObject value)
        {
            try
            {
                if (!IsAuthorized())
                {
                    return StatusCode(HttpStatusCode.Unauthorized);
                }

                if (value.Type == JTokenType.Array)
                {
                    return PostMultiple(value.ToObject<JArray>());
                }

                return PostSingle(value);
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, ex.ToString());
            }
        }

        [Route("api/titles/bytitle/{id}")]
        public IHttpActionResult DeletePeople(int id)
        {
            return DeleteById(TABLE_NAME, ID_TITLE_NAME, id);
        }

        [Route("api/titles/byperson/{id}")]
        public IHttpActionResult DeleteTitles(int id)
        {
            return DeleteById(TABLE_NAME, ID_PERSON_NAME, id);
        }
    }
}
