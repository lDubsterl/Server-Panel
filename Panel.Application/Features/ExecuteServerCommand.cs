using Docker.DotNet;
using Docker.DotNet.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Panel.Application.DTOs.ServerRequests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Panel.Application.Features
{
	public class ExecuteServerCommandHandler : IRequestHandler<ExecuteServerCommandRequest, IActionResult>
	{
		public async Task<IActionResult> Handle(ExecuteServerCommandRequest request, CancellationToken cancellationToken)
		{
			
		}
	}
}
