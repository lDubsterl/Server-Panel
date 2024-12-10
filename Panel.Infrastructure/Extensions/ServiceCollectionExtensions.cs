﻿using Panel.Infrastructure.Services;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Panel.Infrastructure.Extensions
{
	public static class ServiceCollectionExtensions
	{
		public static void AddInfrastructureLayer(this IServiceCollection services)
		{
			services.AddServices();
		}

		private static void AddServices(this IServiceCollection services)
		{
			services
				.AddTransient<IMediator, Mediator>()
				.AddTransient<IAuthenticationService, AuthenticationService>()
				.AddTransient<ITokenService, TokenService>();
		}
	}
}
