using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Panel.Application.DTOs.ServerRequests;
using Panel.Application.Features.ServerInteraction;
using Panel.Application.Features.ServerInteraction.DST;
using Panel.Application.Features.ServerInteraction.Minecraft;
using Panel.Application.Interfaces.Services;
using Panel.Domain.Common;
using System.Threading.Tasks;

namespace ServerPanel.Controllers
{

	[Route("api/[controller]/{id}/[action]")]
	[ApiController, Authorize]
	public class ServerSelectionController : BaseController
	{
		private readonly IConfiguration _config;
		private readonly IMediator _mediator;
		public ServerSelectionController(IConfiguration config, IMediator mediator)
		{
			_config = config;
			_mediator = mediator;
		}

		[HttpPut]
		public async Task<IActionResult> CreateMinecraftServer(string serverVersion)
		{
			return await _mediator.Send(new CreateMinecraftServerRequest(serverVersion, UserId));
		}

		[HttpPut]
		public async Task<IActionResult> CreateDSTServer(CreateDSTServerRequest request)
		{
			return await _mediator.Send(request);
		}

		[HttpDelete]
		public async Task<IActionResult> DeleteMinecraftServer()
		{
			return await _mediator.Send(new DeleteMinecraftServer(UserId));
		}

		[HttpDelete]
		public async Task<IActionResult> DeleteDSTServer()
		{
			return await _mediator.Send(new DeleteDSTServer(UserId));
		}

		[HttpGet]
		public async Task<IActionResult> StartMinecraftServer()
		{
			return await _mediator.Send(new StartMinecraftServer(UserId));
		}

		[HttpGet]
		public async Task<IActionResult> StartDSTServer()
		{
			return await _mediator.Send(new StartDSTServer(UserId));
		}

		[HttpGet]
		public async Task<IActionResult> GetServerStatus()
		{
			return await _mediator.Send(new GetServerStatusRequest(UserId));
		}

		[HttpGet]
		public async Task<IActionResult> GetSettings(ServerTypes type)
		{
			return await _mediator.Send(new GetServerSettingsRequest(UserId, type));
		}

		[HttpPatch]
		public async Task<IActionResult> UpdateSettings(string[] settings)
		{
			return await _mediator.Send(new UpdateMinecraftSettingsRequest(UserId, settings));
		}

		//[HttpGet]
		//public object ParseFile(int id, [Required] string name)
		//{
		//	UserAccount user = GetUser(id);
		//	name = $"{_serversRoot}{user.Email}\\" + name;
		//	if (System.IO.File.Exists(name))
		//	{
		//		return System.IO.File.ReadAllText(name);
		//	}
		//	if (System.IO.Directory.Exists(name))
		//	{
		//		var content = System.IO.Directory.GetFileSystemEntries(name);
		//		var formattedPathes = content;
		//		int i = 0;
		//		foreach (var str in content)
		//		{
		//			formattedPathes[i++] = str.Replace(@"\\", @"\");
		//		}
		//		return formattedPathes;
		//	}
		//	return "";
		//}

		//[HttpPatch]
		//public StatusCodeResult WriteFile(int id, [Required] string name, string fileContent)
		//{
		//	UserAccount user = GetUser(id);
		//	var text = fileContent.Split("\\r\\n");
		//	System.IO.File.WriteAllLines($"{_serversRoot}{user.Email}\\{name}", text);
		//	return new StatusCodeResult((int)HttpStatusCode.OK);
		//}
	}
}
