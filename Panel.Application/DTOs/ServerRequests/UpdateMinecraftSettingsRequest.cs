using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Panel.Application.DTOs.ServerRequests
{
	public class UpdateMinecraftSettingsRequest(int id, string[] settings) : IRequest<IActionResult>
	{
		public int Id { get; set; } = id;
		public string[] Settings { get; set; } = settings;
	}
}
