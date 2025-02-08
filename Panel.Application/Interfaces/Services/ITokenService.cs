using Panel.Application.AuthenticationRequests;
using Microsoft.AspNetCore.Mvc;
using Panel.Domain.Models;

namespace Panel.Application.Interfaces.Services
{
	public interface ITokenService
	{
		Task<string> GenerateAccessTokenAsync(int userId);
		Task<Tokens> GenerateTokensAsync(int userId);
		Task<object> ValidateRefreshTokenAsync(TokenDTO refreshTokenRequest);
		Task<bool> RemoveRefreshTokenAsync(UserAccount user);
	}
}
