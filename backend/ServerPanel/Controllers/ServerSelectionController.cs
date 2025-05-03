using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Panel.Application.DTOs.ServerRequests;
using Panel.Application.Features.ServerInteraction;
using Panel.Application.Features.ServerInteraction.DST;
using Panel.Application.Features.ServerInteraction.Minecraft;
using Panel.Application.Features.ServerInteraction.Terraria;
using Panel.Domain.Common;
using System.Threading.Tasks;

namespace ServerPanel.Controllers
{

    [Route("api/[controller]")]
    [ApiController, Authorize]
    public class ServerSelectionController(IMediator mediator) : BaseController
    {
        private readonly IMediator _mediator = mediator;

        [HttpPut("2")]
        public async Task<IActionResult> CreateMinecraftServer(string serverVersion)
        {
            return await _mediator.Send(new CreateMinecraftServerRequest(serverVersion, UserId));
        }

        [HttpPut("0")]
        public async Task<IActionResult> CreateDSTServer(CreateDSTServerRequest request)
        {
            request.Id = UserId;
            return await _mediator.Send(request);
        }

        [HttpPut("3")]
        public async Task<IActionResult> CreateTerrariaServer([FromBody]CreateTerrariaServerRequest request)
        {
            request.UserId = UserId;
            return await _mediator.Send(request);
        }

        [HttpDelete("{serverType}")]
        public async Task<IActionResult> DeleteServer(ServerTypes serverType)
        {
            return serverType switch
            {
                ServerTypes.Minecraft => await _mediator.Send(new DeleteMinecraftServer(UserId)),
                ServerTypes.DstMaster => await _mediator.Send(new DeleteDSTServer(UserId)),
                ServerTypes.Terraria => await _mediator.Send(new DeleteTerrariaServer(UserId)),
                _ => NotFound(),
            };
        }

        [HttpGet("{serverType}")]
        public async Task<IActionResult> StartServer(ServerTypes serverType)
        {
            return serverType switch {
                ServerTypes.Minecraft => await _mediator.Send(new StartMinecraftServer(UserId)),
                ServerTypes.DstMaster => await _mediator.Send(new StartDSTServer(UserId)),
                ServerTypes.Terraria => await _mediator.Send(new StartTerrariaServer(UserId)),
                _ => NotFound(),
            };
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> GetServerStatus(ServerTypes serverType)
        {
            return await _mediator.Send(new GetServerStatusRequest(UserId, serverType));
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> GetSettings(ServerTypes type)
        {
            return await _mediator.Send(new GetServerSettingsRequest(UserId, type));
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> GetSetModList(GetSetModList request)
        {
            request.UserId = UserId;
            return await _mediator.Send(request);
        }

        [HttpPatch("[action]")]
        public async Task<IActionResult> UpdateFile(UpdateFileRequest request)
        {
            return await _mediator.Send(new UpdateFileRequest(UserId, request.Path, request.Content));
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> GetFolderContent(string path)
        {
            return await _mediator.Send(new GetContentRequest(UserId, path));
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> GetInitialConfig([FromQuery]GetConfigRequest request)
        {
            return await _mediator.Send(request);
        }
    }
}
