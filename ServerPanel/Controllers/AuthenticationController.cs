using Library.Application.Features.Authentication;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Panel.Application.DTOs.AuthenticationRequests;
using System.Threading.Tasks;

namespace ServerPanel.Controllers
{
    [Route("api/[controller]")]
	[ApiController]
	public class AuthenticationController : BaseController
	{
		IMediator _mediator;

		public AuthenticationController(IMediator mediator)
		{
			_mediator = mediator;
		}

		[HttpPost("Login")]
		public async Task<IActionResult> LogIn(Login request)
		{
			return await _mediator.Send(request);
		}

		[HttpPost("SignUp")]
		public async Task<IActionResult> SignUp(SignUp request)
		{
			return await _mediator.Send(request);
		}

		[HttpPost("Reauthorize")]
		public async Task<IActionResult> GetNewAccessToken(GetAccessToken request)
		{
			return await _mediator.Send(request);
		}

		[HttpPost("Logout")]
		public async Task<IActionResult> LogOut()
		{
			return await _mediator.Send(new LogOutRequest(UserId));
		}

		[HttpGet("Verify")]
		public IActionResult Verify()
		{
			if (HttpContext.User.Identity.IsAuthenticated)
				return Ok(Role == "admin");
			return Unauthorized();
		}
	}
}
