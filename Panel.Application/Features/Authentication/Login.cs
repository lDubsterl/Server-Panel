using MediatR;
using Microsoft.AspNetCore.Mvc;
using Panel.Application.DTOs.AuthenticationRequests;
using Panel.Application.Interfaces.Services;

namespace Library.Application.Features.Authentication
{

    public class LoginHandler : IRequestHandler<Login, IActionResult>
	{
		IAuthenticationService _service;
		public LoginHandler(IAuthenticationService service)
		{
			_service = service;
		}
		public async Task<IActionResult> Handle(Login request, CancellationToken cancellationToken)
		{
			return await _service.LoginAsync(request);
		}
	}
}
