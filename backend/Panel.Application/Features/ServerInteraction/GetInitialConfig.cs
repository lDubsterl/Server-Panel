using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Panel.Domain.Common;

namespace Panel.Application.Features.ServerInteraction
{
	public class GetConfigRequest : IRequest<IActionResult>
	{
		public ServerTypes ServerType { get; set; }
	}
	public class GetInitialConfig(IConfiguration config) : IRequestHandler<GetConfigRequest, IActionResult>
	{
		IConfiguration _config = config;

		public async Task<IActionResult> Handle(GetConfigRequest request, CancellationToken cancellationToken)
		{
			var result = request.ServerType switch
			{
				ServerTypes.DstMaster => File.ReadAllLines(_config["ServersDirectory"] + "/DST templates/master_worldgen.txt"),
				ServerTypes.Terraria => File.ReadAllLines(_config["ServersDirectory"] + "/Terraria/tModLoader/serverconfig.txt"),
				ServerTypes.Minecraft => Directory.GetFiles(_config["ServersDirectory"] + "/Minecraft/").Select(Path.GetFileNameWithoutExtension),
				_ => []
			};
			return await Task.FromResult(new OkObjectResult(result));
		}
	}
}
