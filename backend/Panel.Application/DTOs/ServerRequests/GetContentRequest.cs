using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Panel.Application.DTOs.ServerRequests
{
	public class GetContentRequest : IRequest<IActionResult>
	{
		public GetContentRequest(int userId, string path)
		{
			UserId = userId;
			Path = path;
		}

		public int UserId { get; }
		public string Path { get; }
	}
}
