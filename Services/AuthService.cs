using BLPCirculatingSupply.Helpers;

namespace BLPCirculatingSupply.Services
{
    public class AuthService(IConfiguration configuration) : IAuthService
    {
        readonly IConfiguration _configuration = configuration;

        /// <summary>
        /// Basic login method that allows any username/password combo
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns> a jwt token with username as the identifier/claim </returns>
        /// <exception cref="Exception"></exception>
        public string Login(string username, string password)
        {
            string? jwtToken = _configuration.GetValue<string>("Jwt:Secret") ?? throw new Exception("Unable to generate an authentication token");
            return JwtTokenGenerator.GenerateJwtToken(username, jwtToken);
        }
    }
}
