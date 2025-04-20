using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

		public UserController(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
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
	}
}
