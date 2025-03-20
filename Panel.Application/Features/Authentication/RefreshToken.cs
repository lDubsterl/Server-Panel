using MediatR;
using Microsoft.AspNetCore.Mvc;
using Panel.Application.Interfaces.Services;
using Panel.Domain.Common;

namespace Library.Application.Features.Authentication
{
	public class RefreshToken : Token, IRequest<IActionResult> { }
	public class RefreshTokenHandler : IRequestHandler<RefreshToken, IActionResult>
	{
		ITokenService _service;

		public RefreshTokenHandler(ITokenService tokenService)
		{
			this._service = tokenService;
		}

		public async Task<IActionResult> Handle(RefreshToken request, CancellationToken cancellationToken)
		{
			if (request == null || string.IsNullOrEmpty(request.JwtToken) || request.UserId == 0)
				return new BadRequestObjectResult(new { Message = "Missing refresh token details" });

			var validateRefreshTokenResponse = await _service.ValidateRefreshTokenAsync(request);

			if (validateRefreshTokenResponse is BadRequestResult fail)
				return fail;

			var userId = (int)validateRefreshTokenResponse;
			var tokenResponse = await _service.GenerateTokensAsync(userId);

			return new OkObjectResult(new { Data = tokenResponse, Message = "Tokens were refreshed successfully" });
		}
	}
}
