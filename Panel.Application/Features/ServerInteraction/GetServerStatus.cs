using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Panel.Application.Interfaces.Services;
using Panel.Domain.Common;
using Panel.Domain.Interfaces.Repositories;
using Panel.Domain.Models;

namespace Panel.Application.Features.ServerInteraction
{
	public class GetServerStatusRequest : IRequest<IActionResult>
	{
		public int UserId { get; set; }
		public ServerTypes ServerType { get; set; }

		public GetServerStatusRequest(int id, ServerTypes type)
		{
			UserId = id;
			ServerType = type;
		}
	}

	public class GetServerStatusHandler : IRequestHandler<GetServerStatusRequest, IActionResult>
	{
		IUnitOfWork _unitOfWork;
		IOsInteraction _processManager;

		public GetServerStatusHandler(IUnitOfWork unitOfWork, IOsInteraction processManager)
		{
			_unitOfWork = unitOfWork;
			_processManager = processManager;
		}

		public async Task<IActionResult> Handle(GetServerStatusRequest request, CancellationToken cancellationToken)
		{
			var server = await _unitOfWork.Repository<RunningServer>().Entities
				.Where(s => s.UserId == request.UserId && s.ServerType == request.ServerType)
				.FirstOrDefaultAsync();
			var user = await _unitOfWork.Repository<User>().GetByIdAsync(request.UserId);

			if (user == null)
				return new BadRequestObjectResult(new BaseResponse(false, "User not found"));
			bool isExists = request.ServerType switch
			{
				ServerTypes.DstMaster => user.DSTServer,
				ServerTypes.Terraria => user.TerrariaServer,
				ServerTypes.Minecraft => !string.IsNullOrEmpty(user.MinecraftServerExecutable),
				_ => false
			};
			if (!isExists)
				return new BadRequestResult();
			if (server == null)
				return new OkObjectResult(new { isStarted = false });

			if (request.ServerType == ServerTypes.Minecraft || request.ServerType == ServerTypes.Terraria)
				return new OkObjectResult(new
				{
					isStarted = true,
					address = _processManager.ExecuteCommand("curl ifconfig.me") + ":" + server.Port,
					containerName = server.ContainerName
				});
			else
				return new OkObjectResult(new
				{
					isStarted = true,
					containerName = server.ContainerName
				});
		}
	}
}
