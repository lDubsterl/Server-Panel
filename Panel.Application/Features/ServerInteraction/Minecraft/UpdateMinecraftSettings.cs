using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Panel.Application.DTOs.ServerRequests;
using Panel.Application.Interfaces.Services;
using Panel.Domain.Interfaces.Repositories;
using Panel.Domain.Models;
using Panel.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Panel.Application.Features.ServerInteraction.Minecraft
{
	public class UpdateMinecraftSettingsHandler : IRequestHandler<UpdateMinecraftSettingsRequest, IActionResult>
	{
		IConfiguration _config;
		IUnitOfWork _unitOfWork;
		public UpdateMinecraftSettingsHandler(IConfiguration config, IUnitOfWork unitOfWork)
		{
			_config = config;
			_unitOfWork = unitOfWork;
		}

		public async Task<IActionResult> Handle(UpdateMinecraftSettingsRequest request, CancellationToken cancellationToken)
		{
			var user = await _unitOfWork.Repository<User>().GetByIdAsync(request.Id);

			if (user is null || user.MinecraftServerExecutable == "")
				return new BadRequestObjectResult(new BaseResponse(false, "There are no existing servers to change settings"));
			
			var username = user.Email.Replace("@", "");
			var settingsDirectory = $"{_config["ServersDirectory"]}/{username}/Minecraft/server.properties";
			File.WriteAllLines(settingsDirectory, request.Settings);
			return new OkResult();
		}
	}
}
