using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Panel.Application.DTOs.AuthenticationRequests;
using Panel.Application.Features;
using Panel.Domain.Interfaces.Repositories;
using Panel.Domain.Models;
using ServerPanel.Controllers;
using System.Threading.Tasks;

namespace Panel.WebAPI.Controllers
{
	[Route("api/[controller]/[action]")]
	[ApiController]
	[Authorize]
	public class UserController : BaseController
	{
		private IUnitOfWork _unitOfWork;
		private IMediator _mediator;

		public UserController(IUnitOfWork unitOfWork, IMediator mediator)
		{
			_unitOfWork = unitOfWork;
			_mediator = mediator;
		}

		[HttpGet]
		public async Task<IActionResult> GetUserInfo()
		{
			return await _mediator.Send(new GetUserInfoRequest(UserId));
		}

		[HttpGet]
		public async Task<IActionResult> GetServers()
		{
			var user = await _unitOfWork.Repository<User>().GetByIdAsync(UserId);
			return Ok(new { 
				minecraft = !string.IsNullOrEmpty(user.MinecraftServerExecutable),
				dst = user.DSTServer,
				terraria = user.TerrariaServer
			});
		}

		[HttpPost]
		public async Task<IActionResult> ChangePassword(ChangePassword request)
		{
			request.UserId = UserId;
			return await _mediator.Send(request);
		}
	}
}
