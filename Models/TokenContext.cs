using Microsoft.EntityFrameworkCore;

namespace BLPCirculatingSupply.Models
{
    public class TokenContext(DbContextOptions<TokenContext> options): DbContext(options)
    {
        public DbSet<Token> Tokens { get; set;}
    }
}
