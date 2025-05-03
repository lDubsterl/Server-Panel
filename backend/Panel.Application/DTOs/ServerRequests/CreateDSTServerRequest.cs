using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Panel.Application.DTOs.ServerRequests
{
    public class CreateDSTServerRequest : IRequest<IActionResult>
    {
        public int Id { get; set; }
        public string ServerName { get; set; } = "DST";
        public string ServerDescription { get; set; } = "";
        public string ServerPassword { get; set; } = "";
        public string[]? Worldgen { get; set; }
        public string[]? Modlist { get; set; }
        public string? ServerToken { get; set; }
    }
}
