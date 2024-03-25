using BLPCirculatingSupply.Models;

namespace BLPCirculatingSupply.Services
{
    public interface ITokenService
    {
        Task<Token?> GetTokenInfo();
        Task<Token?> SaveTokenInfo(Token Token);
        Task<dynamic> GetTotalSupply(string ContractAddress);
        Task<dynamic> GetTokenBalance(string ContractAddress, string TokenAddress);
    }
}
