using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Panel.Application.DTOs.ServerRequests
{
	public class UpdateFileRequest(int id, string path, string[] content) : IRequest<IActionResult>
	{
		public int Id { get; set; } = id;
		public string Path { get; set; } = path;
		public string[] Content { get; set; } = content;
	}
}
