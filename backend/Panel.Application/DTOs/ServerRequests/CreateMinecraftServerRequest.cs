using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Panel.Application.DTOs.ServerRequests
{
    public class CreateMinecraftServerRequest : IRequest<IActionResult>
    {
        public int Id { get; }
        public string ServerExecutableName { get; }
        public CreateMinecraftServerRequest(string serverExecutableName, int id)
        {
            ServerExecutableName = serverExecutableName;
            Id = id;
        }
    }
}
