using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Panel.Application.DTOs.ServerRequests;
using Panel.Domain.Common;
using Panel.Domain.Interfaces.Repositories;
using Panel.Domain.Models;

namespace Panel.Application.Features.ServerInteraction
{
	public class GetContentHandler : IRequestHandler<GetContentRequest, IActionResult>
	{
		static string[] _readableFilesExtensions = { ".txt", ".json", ".log" };
		IUnitOfWork _unitOfWork;
		IConfiguration _configuration;

		public GetContentHandler(IUnitOfWork unitOfWork, IConfiguration configuration)
		{
			_unitOfWork = unitOfWork;
			_configuration = configuration;
		}

		public async Task<IActionResult> Handle(GetContentRequest request, CancellationToken cancellationToken)
		{
			var user = await _unitOfWork.Repository<User>().GetByIdAsync(request.UserId);
			if (user == null)
				return new NotFoundObjectResult(new BaseResponse(false, "There is no user with such id"));

			var rootDirectory = $"{_configuration["ServersDirectory"]}/{user.Email.Replace("@", "")}/";
			var directory = rootDirectory + request.Path;

			if (Directory.Exists(directory))
			{
				List<string> entries = [];
				foreach (var dir in Directory.GetDirectories(directory))
				{
					var folder = Path.GetRelativePath(rootDirectory, dir);
					entries.Add(folder + '/');
				}
				foreach (var file in Directory.GetFiles(directory))
					entries.Add(Path.GetRelativePath(rootDirectory, file));
				return new OkObjectResult(entries);
			}
			else
			{
				var fileExtension = Path.GetExtension(directory);
				if (_readableFilesExtensions.Contains(fileExtension))
					return new OkObjectResult(File.ReadAllLines(directory));
			}
			return new BadRequestObjectResult(new BaseResponse(false, "This file is not readable"));
		}
	}
}
