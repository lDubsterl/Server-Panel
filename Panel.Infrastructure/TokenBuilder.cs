using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace Panel.Infrastructure
{
	public class TokenBuilder
	{
		public const string Issuer = "LibraryAPI";
		public const string Audience = "LibraryUser";
		public const string Secret = "saujfhaclni67423o17typ239398IBI0p98/dfiksued63qw4kwgqsbewufyssdf";
		public static string GenerateAccessToken(int userId, string role)
		{
			var tokenHandler = new JwtSecurityTokenHandler();

			var key = Convert.FromBase64String(Secret);
			var claimsIdentity = new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, userId.ToString()), new Claim(ClaimTypes.Role, role)]);
			var signingCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature);

			var tokenDescriptor = new SecurityTokenDescriptor
			{
				Subject = claimsIdentity,
				Issuer = Issuer,
				Audience = Audience,
				Expires = DateTime.UtcNow.AddMinutes(15),
				SigningCredentials = signingCredentials
			};

			var token = tokenHandler.CreateToken(tokenDescriptor);
			return tokenHandler.WriteToken(token);
		}
		public static string GenerateRefreshToken()
		{
			var randomBytes = new byte[32];
			var random = RandomNumberGenerator.Create();
			random.GetBytes(randomBytes);
			return Convert.ToBase64String(randomBytes);
		}
	}
}
