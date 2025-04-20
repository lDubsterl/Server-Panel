using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Panel.Application.DTOs.ServerRequests
{
    public class CreateTerrariaServerRequest: IRequest<IActionResult>
    {
        public int UserId { get; set; }
        public string[] Config { get; set; }
    }
}
