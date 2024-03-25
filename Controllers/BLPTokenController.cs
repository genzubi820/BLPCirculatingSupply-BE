using BLPCirculatingSupply.Models;
using BLPCirculatingSupply.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Numerics;

namespace BLPCirculatingSupply.Controllers
{
    public class BLPTokenController(ITokenService TokenService, IConfiguration Configuration) : Controller
    {
        readonly ITokenService TokenService = TokenService;
        readonly IConfiguration Configuration = Configuration;

        /// <summary>
        /// Calculates the Total and Ciculating Supply of BLP Token and saves it in an in-memory database. Will update the previous calculation if present to have the latest supply info
        /// </summary>
        /// <returns>The newly added record in the database</returns>
        [HttpGet]
        [Authorize]
        [Route("api/calculateSupply")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(Token), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Token>> RecalculateBLPSupply()
        {
            try
            {
                // Fetch the total supply from the service
                string BlpContractAddress = Configuration.GetValue<string>("Tokens:BLP:ContractAddress") ?? "";
                BigInteger TotalBLPSupply = await TokenService.GetTotalSupply(BlpContractAddress);

                // Calculate the non-circulating supply
                BigInteger NonCirculatingSupply = await CalculateNonCirculatingSupply();

                // circulating supply = total supply - non-circulating supply
                BigInteger CirculatingSupply = BigInteger.Subtract(TotalBLPSupply, NonCirculatingSupply);

                // create a new Token object and add the information to it
                Token token = new()
                {
                    Name = "BLP", // For simplicity. In future we have can different names passed in as parameter.
                    TotalSupply = TotalBLPSupply.ToString(),
                    CirculatingSupply = CirculatingSupply.ToString()
                };

                // Save the new token info in the in-memory database
                await TokenService.SaveTokenInfo(token);

                // return the new Token
                return Ok(token);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Get the stored info from the in-memory database
        /// </summary>
        /// <returns>The stored token info</returns>
        [HttpGet]
        [Route("api/getInfo")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(Token), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Token>> GetStoredInfo()
        {
            try
            {
                Token? token = await TokenService.GetTokenInfo();

                if (token == null)
                {
                    return BadRequest("Unable to find anything saved. Forgot to calculate first?");
                }

                return Ok(token);
            } catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// sum all the balances of non-circulating token addresses as non-circulating supply of BLP
        /// </summary>
        /// <returns>BigInteger of total non-circulating supply</returns>
        /// <exception cref="Exception"></exception>
        private async Task<BigInteger> CalculateNonCirculatingSupply()
        {
            List<string>? NonCirculatingAddresses = Configuration.GetSection("Tokens:BLP:NonCirculatingAddresses").Get<List<string>>();
            string? BlpContractAddress = Configuration.GetValue<string>("Tokens:BLP:ContractAddress");
            
            // maybe we forgot to add static values in json, prevent anything unexpected
            if (NonCirculatingAddresses is null || BlpContractAddress is null)
            {
                throw new Exception("Unable to get results");
            }

            BigInteger NonCirculatingSupply = new();

            // Loop through the non-circulating tokens and get the token balance.
            foreach (string address in NonCirculatingAddresses)
            {
                BigInteger Balance = await TokenService.GetTokenBalance(BlpContractAddress, address);
                NonCirculatingSupply = BigInteger.Add(NonCirculatingSupply, Balance);
            }

            // Sum of all non-circulating tokens is the Non-circulating supply calculated
            return NonCirculatingSupply;
        }
    }
}
