﻿using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Panel.Application.Common;
using Panel.Domain.Common;
using Panel.Domain.Interfaces.Repositories;
using Panel.Domain.Models;
using Panel.Shared;

namespace Panel.Application.Features
{
	public class DeleteMinecraftServer(int id): ClientId(id), IRequest<IActionResult>
	{

	}

	public class DeleteMinecraftServerHandler: IRequestHandler<DeleteMinecraftServer, IActionResult>
	{
		private IUnitOfWork _unitOfWork;
		private IConfiguration _config;

		public DeleteMinecraftServerHandler(IUnitOfWork unitOfWork, IConfiguration config)
		{
			_unitOfWork = unitOfWork;
			_config = config;
		}

		public async Task<IActionResult> Handle(DeleteMinecraftServer request, CancellationToken cancellationToken)
		{
			var userRepository = _unitOfWork.Repository<UserAccount>();
			var user = await userRepository.GetByIdAsync(request.Id);

			if (user == null) return new BadRequestObjectResult(new BaseResponse(false, "User not found"));

			Directory.Delete(_config["ServersDirectory"] + user.Email.Replace("@", "") + "/Minecraft/", true);

			user.MinecraftServer = false;
			await userRepository.UpdateAsync(user);
			var processesRepository = _unitOfWork.Repository<RunningServer>();
			var server = await processesRepository.Entities.FirstOrDefaultAsync(el => el.UserId == request.Id &&
			el.ServerType == ConsoleTypes.MinecraftServer);
			await processesRepository.DeleteAsync(server);
			await _unitOfWork.Save();

			return new OkObjectResult(new BaseResponse("Server deleted successfully"));
		}
	}
}
