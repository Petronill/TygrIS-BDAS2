using Newtonsoft.Json.Linq;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using TISBackend.Auth;
using TISBackend.Db;

namespace TISBackend.Controllers
{
    public class LoginController : ApiController
    {

        public AuthLevel Post(JObject value)
        {
            return (value != null) ? AuthController.Check(AuthToken.FromJSON(value["auth"] as JObject)) : AuthLevel.NONE;
        }
    }
}
