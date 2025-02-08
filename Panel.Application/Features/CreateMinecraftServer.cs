using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Panel.Application.Common;
using Panel.Application.Interfaces.Services;
using Panel.Domain.Interfaces.Repositories;
using Panel.Domain.Models;
using System.Diagnostics;

namespace Panel.Application.Features
{
	public class CreateMinecraftServer(int id) : ClientId(id), IRequest<IActionResult>
	{
	}

	public class CreateMinecraftServerHandler : IRequestHandler<CreateMinecraftServer, IActionResult>
	{
		private IUnitOfWork _unitOfWork;
		private IConfiguration _config;
		private IProcessManager _processManager;
		public CreateMinecraftServerHandler(IUnitOfWork unitOfWork, IConfiguration config, IProcessManager processManager)
		{
			_unitOfWork = unitOfWork;
			_config = config;
			_processManager = processManager;
		}
		public async Task<IActionResult> Handle(CreateMinecraftServer request, CancellationToken cancellationToken)
		{
			var _serversRoot = _config["ServersDirectory"];
			var repository = _unitOfWork.Repository<UserAccount>();

			var client = await repository.GetByIdAsync(request.Id);

			if (client == null)
				return new NotFoundObjectResult("There is no user with such id");

			if (client.MinecraftServer)
				return new ConflictObjectResult("Server is already created");

			client.MinecraftServer = true;
			var serverDirectory = _serversRoot + client.Email.Replace("@", "") + "/Minecraft/";

			if (!Directory.Exists(serverDirectory))
				Directory.CreateDirectory(serverDirectory);

			var task = repository.UpdateAsync(client);
			await _processManager.ExecuteCommandAsync($"java -jar {serverDirectory}../forge-1.20.4-49.0.33-installer.jar --installServer");
			await _processManager.ExecuteCommandAsync("java @libraries/net/minecraftforge/forge/1.20.4-49.0.33/win_args.txt %*");

			var eula = File.ReadAllText(serverDirectory + "eula.txt");
			eula = eula.Replace("false", "true");
			File.WriteAllText(serverDirectory + "eula.txt", eula);

			await task;
			return new OkObjectResult("Created succesfully");
		}
	}
}
