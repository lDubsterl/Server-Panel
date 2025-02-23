using MediatR;
using Microsoft.AspNetCore.Mvc;
using Panel.Domain.Common;

namespace Panel.Application.DTOs
{
	public class ExecuteServerCommandRequest(int userId, ServerTypes type, string command) : IRequest<IActionResult>
	{
		public int Id { get; set; } = userId;
		public ServerTypes ConsoleType { get; set; } = type;
		public string Command { get; set; } = command;
	}
}
