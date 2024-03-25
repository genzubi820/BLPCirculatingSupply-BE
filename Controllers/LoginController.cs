using BLPCirculatingSupply.Models;
using BLPCirculatingSupply.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.ComponentModel.DataAnnotations;

namespace BLPCirculatingSupply.Controllers
{
    public class LoginController(IAuthService service) : Controller
    {
        /// <summary>
        /// Get JWT Token using any username/password combo
        /// </summary>
        /// <param name="login"></param>
        /// <returns> The JWT token</returns>
        /// <response code="200">Returns the JWT token</response>
        /// <response code="400">Incase unable to produce token</response>
        [HttpPost]
        [Route("api/login")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<object> Login([Required, FromBody] Login login)
        {
            try
            {
                string jwtToken = service.Login(login.Username, login.Password);
                return Ok(new { token = jwtToken });
            } catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
