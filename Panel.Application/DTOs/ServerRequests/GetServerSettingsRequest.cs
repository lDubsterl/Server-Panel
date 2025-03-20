using MediatR;
using Microsoft.AspNetCore.Mvc;
using Panel.Domain.Common;

namespace Panel.Application.DTOs.ServerRequests
{
	public class GetServerSettingsRequest : IRequest<IActionResult>
	{
		public int UserId { get; }
		public ServerTypes ServerType { get; }

		public GetServerSettingsRequest(int userId, ServerTypes type)
		{
			UserId = userId;
			ServerType = type;
		}
	}
}
