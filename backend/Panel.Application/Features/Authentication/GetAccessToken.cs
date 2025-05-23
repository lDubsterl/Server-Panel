﻿using MediatR;
using Microsoft.AspNetCore.Mvc;
using Panel.Application.Interfaces.Services;
using Panel.Domain.Common;

namespace Library.Application.Features.Authentication
{
	public class GetAccessToken : Token, IRequest<IActionResult> { }
	public class AccessTokenHandler : IRequestHandler<GetAccessToken, IActionResult>
	{
		readonly ITokenService _service;

		public AccessTokenHandler(ITokenService service)
		{
			_service = service;
		}

		public async Task<IActionResult> Handle(GetAccessToken request, CancellationToken cancellationToken)
		{
			if (request == null || string.IsNullOrEmpty(request.JwtToken) || request.UserId == 0)
				return new BadRequestObjectResult(new { Message = "Missing refresh token details" });

			var validateRefreshTokenResponse = await _service.ValidateRefreshTokenAsync(request);

			if (validateRefreshTokenResponse is BadRequestObjectResult fail)
				return fail;
			int userId = (int)validateRefreshTokenResponse;
			var tokenResponse = await _service.GenerateAccessTokenAsync(userId);

			return new OkObjectResult(new { Data = tokenResponse, Message = "Token was refreshed successfully" });
		}
	}
}
