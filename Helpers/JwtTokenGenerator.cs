using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BLPCirculatingSupply.Helpers
{
    public class JwtTokenGenerator
    {
        /// <summary>
        /// A helper method to generate a jwt token using claim and jwtsecret
        /// For now using 
        /// </summary>
        /// <param name="claim"></param>
        /// <param name="jwtSecret"></param>
        /// <returns>the generated jwt Token</returns>
        public static string GenerateJwtToken(string claim, string jwtSecret)
        {
            JwtSecurityTokenHandler tokenHandler = new();
            byte[] key = Encoding.ASCII.GetBytes(jwtSecret);

            SecurityTokenDescriptor tokenDescriptor = new()
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new(ClaimTypes.Name, claim),
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
