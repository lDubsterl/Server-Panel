using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Panel.Application.DTOs
{
	public class CreateDSTServerRequest : IRequest<IActionResult>
	{
		public int Id { get; }
		public string ServerName { get; } = "";
		public string ServerDescription { get; } = "";
		public string ServerPassword { get; } = "";
	}
}
