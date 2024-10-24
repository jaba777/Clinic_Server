using Clinic_Server.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Clinic_Server.Helper
{
    public class UserDto
    {
        public int userId { get; set; }
        public string role { get; set; }
    }
    public class AuthHelper
    {
        private readonly IConfiguration _configuration;


        public AuthHelper(IConfiguration configuration)
        {
            this._configuration = configuration;
        }
        public string GenerateJWTToken(Users user)
        {

            var claims = new List<Claim> {
         new Claim("userId", user.id.ToString()),
         new Claim("role", user.role)
    };
            var jwtToken = new JwtSecurityToken(
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddDays(30),
                signingCredentials: new SigningCredentials(
                    new SymmetricSecurityKey(
                       Encoding.UTF8.GetBytes(_configuration["JWT:Key"])
                        ),
                    SecurityAlgorithms.HmacSha256Signature)
                );
            return new JwtSecurityTokenHandler().WriteToken(jwtToken);
        }
        public UserDto VerifyJWTToken(string token)
        {


            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["JWT:Key"]); // Use UTF8 to match GenerateJWTToken
            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero // Set clockskew to zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var userId = int.Parse(jwtToken.Claims.First(x => x.Type == "userId").Value); // Use NameIdentifier
                var role = jwtToken.Claims.First(x => x.Type == "role").Value;


                var obj = new UserDto
                {
                    userId = userId,
                    role = role
                };

                // Return user id and user name from JWT token if validation is successful
                return obj;

            }
            catch
            {
                // Return null if validation fails
                return null;
            }

        }
    }
}
