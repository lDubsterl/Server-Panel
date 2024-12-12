using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Panel.Application.Common;
using Panel.Application.Interfaces.Services;
using Panel.Domain.Interfaces.Repositories;
using Panel.Domain.Models;

namespace Panel.Application.Features
{
	public class CreateMinecraftServer(int id) : ClientId(id), IRequest<IActionResult>
	{
	}

	public class CreateMinecraftServerHandler : IRequestHandler<CreateMinecraftServer, IActionResult>
	{
		private IUnitOfWork _unitOfWork;
		private IConfiguration _config;
		private IProcessManager _manager;
		public CreateMinecraftServerHandler(IUnitOfWork unitOfWork, IConfiguration config, IProcessManager manager)
		{
			_unitOfWork = unitOfWork;
			_config = config;
			_manager = manager;
		}
		public async Task<IActionResult> Handle(CreateMinecraftServer request, CancellationToken cancellationToken)
		{
			var _serversRoot = _config["ServersDirectory"];
			var repository = _unitOfWork.Repository<UserAccount>("Site accounts");

			var client = await repository.GetByIdAsync(request.Id);
			client.MinecraftServer = true;

			if (client.MinecraftServer)
				return new ConflictObjectResult("Server is already created");

			var serverDirectory = _serversRoot + client.Email;

			_manager.ExecuteCommand($"cd /d {_serversRoot} && mkdir {client.Email}");

			var task = repository.UpdateAsync(client);
			_manager.ExecuteCommand(serverDirectory + "java -jar ../forge-1.20.4-49.0.33-installer.jar --installServer");
			_manager.ExecuteCommand(serverDirectory + "java @libraries/net/minecraftforge/forge/1.20.4-49.0.33/win_args.txt %*");

			var eula = _manager.ExecuteCommand(serverDirectory + "type eula.txt");
			eula = eula.Replace("false", "true");
			var strings = eula.Split("\r\n");
			_manager.ExecuteCommand(serverDirectory + $"echo {strings[2]}> eula.txt");

			await task;
			return new OkObjectResult("Created succesfully");
		}
	}
}
