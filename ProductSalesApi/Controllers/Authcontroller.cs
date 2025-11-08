using Microsoft.AspNetCore.Mvc;
using ProductSalesApi.Auth;

namespace ProductSalesApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Authcontroller : ControllerBase
    {
        private readonly JwtHelper _jwt;

        public Authcontroller(JwtHelper jwt)
        {
            _jwt = jwt;
        }

        [HttpPost("login")]
        public IActionResult Login(string username, string password)
        {
            if (username == "admin" && password == "1234")
            {
                var token = _jwt.GenerateToken(username);
                return Ok(new { token });
            }

            return Unauthorized();
        }
    }
}
