using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Panel.Application.DTOs.ServerRequests
{
	public class ExecuteServerCommandRequest: IRequest<IActionResult>
	{
		public string ContainerName { get; set; }
		public string Command { get; set; }
	}
}
