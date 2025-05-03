using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Panel.Application.DTOs.ServerRequests
{
    public class CreateTerrariaServerRequest: IRequest<IActionResult>
    {
        public int UserId { get; set; }
		public string ServerName { get; set; } = "Terraria";
		public string ServerDescription { get; set; } = "";
		public string ServerPassword { get; set; } = "";
		public string[] Config { get; set; }
        public string[] Modlist { get; set; }
    }
}
