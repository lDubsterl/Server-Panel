using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Panel.Application.DTOs.ServerRequests;
using Panel.Domain.Common;
using Panel.Domain.Interfaces.Repositories;
using Panel.Domain.Models;

namespace Panel.Application.Features.ServerInteraction
{
	public class UpdateFileHandler : IRequestHandler<UpdateFileRequest, IActionResult>
    {
        IConfiguration _config;
        IUnitOfWork _unitOfWork;
        public UpdateFileHandler(IConfiguration config, IUnitOfWork unitOfWork)
        {
            _config = config;
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Handle(UpdateFileRequest request, CancellationToken cancellationToken)
        {
            var user = await _unitOfWork.Repository<User>().GetByIdAsync(request.Id);

            if (user is null)
                return new BadRequestObjectResult(new BaseResponse(false, "Server not found"));

            var username = user.Email.Replace("@", "");
            var fileDirectory = $"{_config["ServersDirectory"]}/{username}/" + request.Path;
            File.WriteAllLines(fileDirectory, request.Content);
            return new OkResult();
        }
    }
}
