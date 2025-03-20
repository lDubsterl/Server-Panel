using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Panel.Domain.Common;
using Panel.Domain.Interfaces.Repositories;
using Panel.Domain.Models;

namespace Panel.Application.Features.ServerInteraction.DST
{
	public class DeleteDSTServer(int id) : IRequest<IActionResult>
    {
        public int Id { get; } = id;
    }

    public class DeleteDSTServerHandler : IRequestHandler<DeleteDSTServer, IActionResult>
    {
        private IUnitOfWork _unitOfWork;
        private IConfiguration _config;

        public DeleteDSTServerHandler(IUnitOfWork unitOfWork, IConfiguration config)
        {
            _unitOfWork = unitOfWork;
            _config = config;
        }

        public async Task<IActionResult> Handle(DeleteDSTServer request, CancellationToken cancellationToken)
        {
            var repository = _unitOfWork.Repository<User>();
            var user = await repository.GetByIdAsync(request.Id);
            var dstDedicatedServerRoot = _config["DST_CLI_Directory"];

            if (user == null) return new BadRequestObjectResult(new BaseResponse(false, "User not found"));

            Directory.Delete(_config["ServersDirectory"] + user.Email.Replace("@", "") + "/Minecraft/", true);

            File.Delete(@$"{dstDedicatedServerRoot}Don't Starve Together Dedicated Server\bin\Server_Caves{user.Email}.bat");
            File.Delete(@$"{dstDedicatedServerRoot}Don't Starve Together Dedicated Server\bin\Server_Master{user.Email}.bat");

            user.DSTServer = false;
            await repository.UpdateAsync(user);
            var processesRepository = _unitOfWork.Repository<RunningServer>();
            await processesRepository.Entities.Where(el => el.UserId == request.Id &&
            (el.ServerType == ServerTypes.DstMaster || el.ServerType == ServerTypes.DstCaves))
                .ForEachAsync(async el => await processesRepository.DeleteAsync(el));
            await _unitOfWork.Save();

            return new OkObjectResult(new BaseResponse("Server deleted successfully"));

            //dstServerProcesses.Remove(id);
        }
    }
}
