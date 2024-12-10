using Panel.Application.AuthenticationRequests;
using Microsoft.AspNetCore.Mvc;

namespace Panel.Application.Interfaces.Services
{
	public interface ITokenService
	{
		Task<string> GenerateAccessTokenAsync(int userId);
		Task<Tokens> GenerateTokensAsync(int userId);
		Task<object> ValidateRefreshTokenAsync(TokenDTO refreshTokenRequest);
		Task<bool> RemoveRefreshTokenAsync(User user);
	}
}
