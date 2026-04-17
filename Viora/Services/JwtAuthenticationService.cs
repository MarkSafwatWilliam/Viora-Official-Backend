using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Viora.Models;
using Viora.Repositories;

namespace Viora.Services
{
    public class JwtAuthenticationService
    {
        private readonly IConfiguration _configuration;
        private readonly GenericRepository<RefreshToken> _repository;

        public JwtAuthenticationService(IConfiguration configuration, GenericRepository<RefreshToken> repository)
        {
            _configuration = configuration;
            _repository = repository;
        }

        public async Task<string> GenerateJwtToken(ApplicationUser user ,List<string> roles)
        {
            List<Claim> claims = new List<Claim>()
            {
                new Claim(JwtRegisteredClaimNames.Sub,user.Id.ToString()),

                new Claim(JwtRegisteredClaimNames.Name,user.DisplayName),

                new Claim(JwtRegisteredClaimNames.Email,user.Email),

                new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString())

            };

            foreach (var role in roles) { 
                claims.Add(new Claim("role", role));
            }

            var Key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));

            var creds = new SigningCredentials(Key, SecurityAlgorithms.HmacSha256);

            var validityMins = int.Parse(_configuration["Jwt:TokenValidityMins"]);

            var token = new JwtSecurityToken(
                claims: claims,
                signingCredentials: creds,
                expires: DateTime.UtcNow.AddMinutes(validityMins));

            var stringToken = new JwtSecurityTokenHandler().WriteToken(token);
            return stringToken;
        }


        public async Task<string> GenerateRefreshToken(int userId) { 
        
            var validityDays = int.Parse(_configuration["Jwt:RefreshTokenValidityDays"]);
            RefreshToken refreshToken = new RefreshToken()
            {

                UserId = userId,
                Token = Guid.NewGuid().ToString(),
                CreatedAt = DateTime.UtcNow,
                ExpiredAt = DateTime.UtcNow.AddDays(validityDays),
                IsRevoked = false
            };

            await _repository.Add(refreshToken);
            await _repository.SaveChanges();
            return refreshToken.Token;
        }
        
    }
}
