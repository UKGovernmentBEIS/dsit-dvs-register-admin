using DVSAdmin.CommonUtility.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

namespace DVSAdmin.CommonUtility.JWT
{
    public class JwtService :IJwtService
    {
        private readonly JwtSettings jwtSettings;
        private readonly IConfiguration configuration;
        private readonly ILogger<JwtService> logger;

        public JwtService(IOptions<JwtSettings> jwtSettings, IConfiguration configuration, ILogger<JwtService> logger)
        {
            this.jwtSettings = jwtSettings.Value;
            this.configuration = configuration;
            this.logger = logger;
        }

        public TokenDetails GenerateToken(string audience = "", int providerId = 0, List<int>? serviceIds = null)
        {
            TokenDetails tokenDetails = new TokenDetails();
            tokenDetails.TokenId  = Guid.NewGuid().ToString();
            var claims = new List<Claim>()
            {
                new(JwtRegisteredClaimNames.Website,configuration["ReviewPortalLink"]),
                new(JwtRegisteredClaimNames.Jti,   tokenDetails.TokenId)
            };

            if (providerId > 0)
            {
                claims.Add(new Claim("ProviderProfileId", providerId.ToString()));
            }

            if (serviceIds != null && serviceIds.Count > 0)
            {
                foreach (var serviceId in serviceIds)
                {
                    claims.Add(new Claim("ServiceId", serviceId.ToString()));
                }
            }
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

            var handler = new JsonWebTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(jwtSettings.ExpiryMinutes),
                SigningCredentials = credentials,
                Issuer = jwtSettings.Issuer,
                Audience = string.IsNullOrEmpty(audience)?jwtSettings.Audience: "DSIT"
            };
            var token = handler.CreateToken(tokenDescriptor);
            tokenDetails.Token = token;
            return tokenDetails;
        }

        
    }
}
