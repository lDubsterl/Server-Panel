using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Panel.Application.DTOs.ServerRequests;
using Panel.Domain.Common;
using Panel.Domain.Interfaces.Repositories;
using Panel.Domain.Models;

namespace Panel.Application.Features.ServerInteraction
{

	public class GerServerSettingsHandler : IRequestHandler<GetServerSettingsRequest, IActionResult>
    {
        IUnitOfWork _unitOfWork;
        IConfiguration _configuration;

        public GerServerSettingsHandler(IUnitOfWork unitOfWork, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
        }

        public async Task<IActionResult> Handle(GetServerSettingsRequest request, CancellationToken cancellationToken)
        {
            var user = await _unitOfWork.Repository<User>().GetByIdAsync(request.UserId);
            if (user == null)
                return new NotFoundObjectResult(new BaseResponse(false, "There is no user with such id"));

            if (string.IsNullOrEmpty(user.MinecraftServerExecutable))
                return new NoContentResult();

            var settingsDirectory = $"{_configuration["ServersDirectory"]}/{user.Email.Replace("@", "")}/Minecraft/server.properties";
            return new OkObjectResult(File.ReadAllLines(settingsDirectory));
        }
    }
}
