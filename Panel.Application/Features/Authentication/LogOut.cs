using MediatR;
using Microsoft.AspNetCore.Mvc;
using Panel.Application.Interfaces.Services;

namespace Library.Application.Features.Authentication
{
	public record LogOutRequest : IRequest<IActionResult>
	{
		public LogOutRequest(int id)
		{
			Id = id;
		}

		public int Id { get; set; }

	}
	public class LogOutHandler : IRequestHandler<LogOutRequest, IActionResult>
	{
		IAuthenticationService _service;

		public LogOutHandler(IAuthenticationService service)
		{
			_service = service;
		}

		public async Task<IActionResult> Handle(LogOutRequest request, CancellationToken cancellationToken)
		{
			return await _service.LogoutAsync(request.Id);
		}
	}
}
