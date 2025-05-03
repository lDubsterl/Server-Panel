using Panel.Application.DTOs.AuthenticationRequests;
using Panel.Domain.Common;
using Panel.Domain.Models;

namespace Panel.Application.Interfaces.Services
{
	public interface ITokenService
	{
		Task<string> GenerateAccessTokenAsync(int userId);
		Task<Tokens> GenerateTokensAsync(int userId);
		Task<object> ValidateRefreshTokenAsync(Token refreshTokenRequest);
		Task<bool> RemoveRefreshTokenAsync(User user);
	}
}
