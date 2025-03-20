using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Panel.Application.Interfaces.Services;
using Panel.Domain.Interfaces.Repositories;
using Panel.Infrastructure.Repositories;
using Panel.Infrastructure.Services;

namespace Panel.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
	{
		public static void AddInfrastructureLayer(this IServiceCollection services, IConfiguration config)
		{
			services.AddServices();
			services.AddDbContext(config);
			services.AddRepositories();
		}

		private static void AddServices(this IServiceCollection services)
		{
			services
				.AddTransient<IMediator, Mediator>()
				.AddTransient<IAuthenticationService, AuthenticationService>()
				.AddTransient<ITokenService, TokenService>()
				.AddSingleton<IFtpManager, FtpManager>()
				.AddSingleton<IOsInteractionsService, OsInteractionsService>();
		}

		public static void AddDbContext(this IServiceCollection services, IConfiguration configuration)
		{
			var connectionString = configuration["DatabaseConnectionString"];

			services.AddDbContext<PanelDbContext>(options =>
			   options.UseNpgsql(connectionString));
		}

		private static void AddRepositories(this IServiceCollection services)
		{
			services
				.AddTransient<IUnitOfWork, UnitOfWork>()
				.AddTransient(typeof(IRepository<>), typeof(Repository<>));
		}
	}
}
