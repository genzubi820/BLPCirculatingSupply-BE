using BLPCirculatingSupply.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System.Numerics;

namespace BLPCirculatingSupply.Services
{
    public class TokenService(TokenContext tokenContext, IConfiguration configuration) : ITokenService
    {
        private readonly IConfiguration _configuration = configuration;

        /// <summary>
        /// Save the new Token information into context
        /// update if already a token info present
        /// </summary>
        /// <param name="Token"></param>
        /// <returns> the new or updated token information</returns>
        /// <exception cref="Exception"></exception>
        public async Task<Token?> SaveTokenInfo(Token Token)
        {
            try
            {
                // Check if we already have token info
                Token? OldToken = await GetTokenInfo();

                if (OldToken == null)
                {
                    await tokenContext.AddAsync(Token);
                } else
                {
                    // demo purposes. keep only one copy of updated token info
                    // Alternatively save updated info as new record and fetch latest when needed
                    Token.TokenId = OldToken.TokenId;
                    OldToken.Name = Token.Name;
                    OldToken.TotalSupply = Token.TotalSupply;
                    OldToken.CirculatingSupply = Token.CirculatingSupply;

                    // Update the information in context, leave ID as is
                    tokenContext.Update(OldToken);
                }
                
                await tokenContext.SaveChangesAsync();
                return Token;
            } catch (Exception ex)
            {
                // Hide inner exception and make error more user friendly
                throw new Exception("Unable to save new token", ex);
            }
        }

        /// <summary>
        /// Get the first token from the context
        /// </summary>
        /// <returns> the first entry of the Tokens DbSet or null </returns>
        /// <exception cref="Exception"></exception>
        public async Task<Token?> GetTokenInfo()
        {
            try
            {
                // first entry has tokenId 1, using that.
                // Not the most concrete way to get the first record but keeping it simple
                return await tokenContext.Tokens.FirstOrDefaultAsync(token => token.TokenId == 1);
            } catch (Exception ex)
            {
                // Hide inner exception and make error more user friendly
                throw new Exception("unable to fetch token", ex);
            }
        }

        /// <summary>
        /// Make an api call to BscScan api and get total supply of the smart contract
        /// </summary>
        /// <param name="ContractAddress"></param>
        /// <returns>Big Integer total supply or Error string from api</returns>
        public async Task<dynamic> GetTotalSupply(string ContractAddress)
        {
            string Url = _configuration.GetValue<string>("BscScan:ApiUrl") + "?module=stats&action=tokensupply&contractaddress=" + ContractAddress + "&apikey=" + _configuration.GetValue<string>("BscScan:ApiKey");
            string result = await MakeGetApiCall(Url);
            return TokenService.ParseBScanAPIResponse(result);
        }

        /// <summary>
        /// Make an api call to BsScan api and get token balance of any token of the smart contract
        /// </summary>
        /// <param name="ContractAddress"></param>
        /// <param name="TokenAddress"></param>
        /// <returns>Big Integer token balance or Error string from api</returns>
        public async Task<dynamic> GetTokenBalance(string ContractAddress, string TokenAddress)
        {
            string Url = _configuration.GetValue<string>("BscScan:ApiUrl") + "?module=account&action=tokenbalance&contractaddress=" + ContractAddress + "&address=" + TokenAddress + "&tag=latest&apikey=" + _configuration.GetValue<string>("BscScan:ApiKey");
            string result = await MakeGetApiCall(Url);
            return TokenService.ParseBScanAPIResponse(result);
        }

        /// <summary>
        /// Helper method to make a GET API call, to remove code duplication.
        /// Can also be placed in helpers but decided to keep in this class for now
        /// </summary>
        /// <param name="Url"></param>
        /// <returns> api responce in a string </returns>
        private static async Task<string> MakeGetApiCall(string Url)
        {
            var Client = new HttpClient();
            var Response = await Client.GetAsync(Url);
            Response.EnsureSuccessStatusCode();
            return await Response.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// Since BscScan api has the same response structure.
        /// It makes sense to have one method to parse it according to application need.
        /// </summary>
        /// <param name="Response"></param>
        /// <returns>Parsed value of the api response</returns>
        /// <exception cref="Exception"></exception>
        private static dynamic ParseBScanAPIResponse(string Response)
        {
            JObject? JsonResponse = JObject.Parse(Response);
            
            if (Response == null || JsonResponse == null)
            {
                throw new Exception("Unable to get results");
            }

            string? Status = JsonResponse.GetValue("status")?.ToString();
            string? Result = JsonResponse.GetValue("result")?.ToString();

            if (Status == "1" && Result != null)
            {
                // results from api won't always be Big Integer. For now keeping it simple
                return BigInteger.Parse(Result);
            } else
            {
                // Status is 0, meaning error
                return "Error: " + Result;
            }
        }
    }
}
