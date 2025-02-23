using Microsoft.AspNetCore.Mvc;
using Panel.Domain.Models;
using Panel.Application.DTOs.AuthenticationRequests;

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
