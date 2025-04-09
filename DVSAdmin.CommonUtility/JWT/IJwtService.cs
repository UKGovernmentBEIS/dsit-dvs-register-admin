using DVSAdmin.CommonUtility.Models;

namespace DVSAdmin.CommonUtility.JWT
{
    public interface IJwtService
    {
        public TokenDetails GenerateToken(string audience = "", int providerId = 0, string serviceIds = ""); 
    }
}
