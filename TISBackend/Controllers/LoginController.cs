using System.Web.Http;
using TISBackend.Auth;
using TISBackend.Db;

namespace TISBackend.Controllers
{
    public class LoginController : ApiController
    {
        // GET: api/Login
        public AuthLevel Get()
        {
            return AuthController.Check(AuthToken.From(Request.Headers));
        }
    }
}
