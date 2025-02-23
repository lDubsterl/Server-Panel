using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Panel.Application.DTOs.ServerRequests
{
	public class CreateDSTServerRequest : IRequest<IActionResult>
    {
        public int Id { get; }
        public string ServerName { get; } = "";
        public string ServerDescription { get; } = "";
        public string ServerPassword { get; } = "";
    }
}
