using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TaskMirror.Models;

namespace TaskMirror.Auth
{
    public class TokenService
    {
        private readonly string _key;

        public TokenService(IConfiguration config)
        {
            _key = config["Jwt:Key"] ?? throw new Exception("Jwt:Key não configurada.");
        }

        public string GenerateToken(Usuario usuario)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var keyBytes = Encoding.ASCII.GetBytes(_key);

            var claims = new List<Claim>
            {
                new Claim("idUsuario", usuario.IdUsuario.ToString()),
                new Claim(ClaimTypes.Role, usuario.RoleUsuario)
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(6),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(keyBytes),
                    SecurityAlgorithms.HmacSha256Signature
                )
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
