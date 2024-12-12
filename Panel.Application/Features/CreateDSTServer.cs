﻿using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Panel.Application.Interfaces.Services;
using Panel.Domain.Interfaces.Repositories;
using Panel.Domain.Models;

namespace Panel.Application.Features
{
	public class CreateDSTServer : IRequest<IActionResult>
	{
		public int Id { get; set; }
		public required string ServerName { get; set; }
		public string ServerDescription { get; set; } = "";
		public string ServerPassword { get; set; } = "";
	}

	public class CreateDSTServerHandler : IRequestHandler<CreateDSTServer, IActionResult>
	{
		IConfiguration _config;
		IUnitOfWork _unitOfWork;
		IProcessManager _processManager;

		public CreateDSTServerHandler(IConfiguration config, IUnitOfWork unitOfWork, IProcessManager processManager)
		{
			_config = config;
			_unitOfWork = unitOfWork;
			_processManager = processManager;
		}

		public async Task<IActionResult> Handle(CreateDSTServer request, CancellationToken cancellationToken)
		{
			var repository = _unitOfWork.Repository<UserAccount>("Site accounts");
			UserAccount accUser = await repository.GetByIdAsync(request.Id);

			if (accUser.DSTServer)
				return new ConflictObjectResult("Server is already created");

			var serversRoot = _config["ServersDirectory"];
			var serverDirectory = _config["DSTServerDirectory"];
			var dstDedicatedServerRoot = _config["DST_CLI_Directory"];
			Directory.CreateDirectory(serverDirectory + accUser.Email);
			File.Copy(serverDirectory + "cluster_token.txt", serverDirectory + accUser.Email + "\\cluster_token.txt");
			serverDirectory += accUser.Email + "\\";
			Directory.CreateDirectory(serverDirectory + "Master");
			Directory.CreateDirectory(serverDirectory + "Caves");

			accUser.DSTServer = true;
			var task = repository.UpdateAsync(accUser);

			var ini = File.ReadAllText($"{serversRoot}DST templates\\master_server.txt");
			File.WriteAllText(serverDirectory + "Master\\server.ini", ini);
			ini = File.ReadAllText($"{serversRoot}DST templates\\caves_server.txt");
			File.WriteAllText(serverDirectory + "Caves\\server.ini", ini);
			ini = File.ReadAllText($"{serversRoot}DST templates\\caves_worldgen.txt");

			File.WriteAllText(serverDirectory + "Caves\\worldgenoverride.lua", ini);

			var cluster = File.ReadAllLines($"{serversRoot}DST templates\\cluster_template.txt");
			cluster[13] = $"cluster_name = {request.ServerName}";
			cluster[15] = $"cluster_description = {request.ServerDescription}";
			cluster[19] = $"cluster_password = {request.ServerPassword}";
			File.WriteAllLines(serverDirectory + "cluster.ini", cluster);

			var server = File.ReadAllText($"{serversRoot}DST templates\\Server_Master.txt");
			server = server.Replace("MyDediServer", accUser.Email);
			File.WriteAllText($"{dstDedicatedServerRoot}Don't Starve Together Dedicated Server\\bin\\Server_Master{accUser.Email}.bat", server);
			server = File.ReadAllText($"{serversRoot}DST templates\\Server_Caves.txt");
			server = server.Replace("MyDediServer", accUser.Email);
			File.WriteAllText($"{dstDedicatedServerRoot}Don't Starve Together Dedicated Server\\bin\\Server_Caves{accUser.Email}.bat", server);

			await task;
			return new OkObjectResult("Created succesfully");
		}
	}
}