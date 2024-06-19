using DVSAdmin.CommonUtility.Models;

namespace DVSAdmin.CommonUtility.JWT
{
    public interface IJwtService
    {
        public TokenDetails GenerateToken();
        public Task<TokenDetails> ValidateToken(string token);
    }
}
