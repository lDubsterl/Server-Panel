using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Panel.Application.DTOs.AuthenticationRequests;
using Panel.Application.Interfaces.Services;
using Panel.Domain.Models;

namespace Panel.Infrastructure.Services
{
    public class TokenService : ITokenService
	{
		readonly PanelDbContext _db;

		public TokenService(PanelDbContext db)
		{
			_db = db;
		}

		public async Task<string> GenerateAccessTokenAsync(int userId)
		{
			var userRecord = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
			
			if (userRecord == null)
				return "";

			var accessToken = TokenBuilder.GenerateAccessToken(userId, userRecord.Role);
			return accessToken;
		}

		public async Task<Tokens> GenerateTokensAsync(int userId)
		{
			var userRecord = await _db.Users.Include(t => t.RefreshTokens).SingleAsync(u => u.Id == userId);

			if (userRecord == null)
				return new Tokens();

			var accessToken = TokenBuilder.GenerateAccessToken(userId, userRecord.Role);
			var refreshToken = TokenBuilder.GenerateRefreshToken();

			var salt = PasswordBuilder.GetSecureSalt();

			var refreshTokenHashed = PasswordBuilder.HashUsingPbkdf2(refreshToken, salt);

			if (userRecord.RefreshTokens != null && userRecord.RefreshTokens.Count != 0)
			{
				await RemoveRefreshTokenAsync(userRecord);

			}
			userRecord.RefreshTokens?.Add(new RefreshToken
			{
				ExpiryDate = DateTime.UtcNow.AddDays(30),
				IssueDate = DateTime.UtcNow,
				UserId = userId,
				TokenHash = refreshTokenHashed,
				TokenSalt = Convert.ToBase64String(salt)

			});

			await _db.SaveChangesAsync();

			var token = new Tokens { AccessToken = accessToken, RefreshToken = refreshToken };

			return token;
		}

		public async Task<bool> RemoveRefreshTokenAsync(User user)
		{
			var userRecord = await _db.Users.Include(o => o.RefreshTokens).FirstOrDefaultAsync(e => e.Id == user.Id);

			if (userRecord == null)
			{
				return false;
			}

			if (userRecord.RefreshTokens != null && userRecord.RefreshTokens.Count != 0)
			{
				var currentRefreshToken = userRecord.RefreshTokens.First();

				_db.Tokens.Remove(currentRefreshToken);
			}

			return false;
		}

		public async Task<object> ValidateRefreshTokenAsync(Token refreshTokenRequest)
		{
			var refreshToken = await _db.Tokens.FirstOrDefaultAsync(o => o.UserId == refreshTokenRequest.UserId);

			if (refreshToken == null)
				return new BadRequestObjectResult(new { Message = "Invalid session or user is already logged out" });

			var refreshTokenToValidateHash = PasswordBuilder.HashUsingPbkdf2(refreshTokenRequest.JwtToken, Convert.FromBase64String(refreshToken.TokenSalt));

			if (refreshToken.TokenHash != refreshTokenToValidateHash)
				return new BadRequestObjectResult(new { Message = "Invalid refresh token" });

			if (refreshToken.ExpiryDate < DateTime.UtcNow)
				return new BadRequestObjectResult(new { Message = "Refresh token has expired" });

			return refreshToken.UserId;
		}
	}
}
