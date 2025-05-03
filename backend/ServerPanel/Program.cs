using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Panel.Application.DTOs.AuthenticationRequests;
using Panel.Infrastructure;
using Panel.Infrastructure.Extensions;
using Panel.Infrastructure.Hubs;
using Panel.Infrastructure.Services;
using StackExchange.Redis;
using System;
using System.Reflection;

namespace ServerPanel
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			var services = builder.Services;
			//builder.WebHost.UseUrls("http://0.0.0.0:5000");
			builder.WebHost.ConfigureKestrel(serverOptions =>
			{
				serverOptions.ListenAnyIP(5001, listenOptions =>
				{
					listenOptions.UseHttps("/mnt/c/users/dubster/.aspnet/https/aspnetapp.pfx", "123");
				});
			});

			services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetAssembly(typeof(Login))));
			services.AddInfrastructureLayer(builder.Configuration);

			services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
					.AddJwtBearer(options =>
					{
						options.RequireHttpsMetadata = false;
						options.TokenValidationParameters = new TokenValidationParameters
						{
							// укзывает, будет ли валидироваться издатель при валидации токена
							ValidateIssuer = true,
							// строка, представляющая издателя
							ValidIssuer = TokenBuilder.Issuer,

							// будет ли валидироваться потребитель токена
							ValidateAudience = true,
							// установка потребителя токена
							ValidAudience = TokenBuilder.Audience,
							// будет ли валидироваться время существования
							ValidateLifetime = true,

							// установка ключа безопасности
							IssuerSigningKey = new SymmetricSecurityKey(Convert.FromBase64String(TokenBuilder.Secret)),
							// валидация ключа безопасности
							ValidateIssuerSigningKey = true,
						};
					});
			services.AddCors(options =>
			{
				options.AddPolicy("CORSPolicy", builder =>
				builder.AllowAnyMethod()
				 .WithOrigins("http://localhost:3000", "https://happiness-rosa-sad-salon.trycloudflare.com")
				.AllowAnyHeader()
				.AllowCredentials());
			});
			services.AddSwaggerGen(c =>
			{
				c.SwaggerDoc("v1", new OpenApiInfo { Title = "ServerPanel", Version = "v1" });
				c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
				{
					Name = "Authorization",
					Type = SecuritySchemeType.ApiKey,
					Scheme = "Bearer",
					BearerFormat = "JWT",
					In = ParameterLocation.Header,
					Description = "Enter JWT token in format [bearer {Token}]"
				});
				c.AddSecurityRequirement(new OpenApiSecurityRequirement
				{
					{
						new OpenApiSecurityScheme
						{
							Reference = new OpenApiReference
							{
								Type = ReferenceType.SecurityScheme,
								Id = "Bearer"
							}
						},
						new string[]{ }
					}
				});
			});

			services.AddAuthorization();
			services.AddControllers().AddNewtonsoftJson();

			var redisConnection = builder.Configuration["RedisConnection"];
			services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConnection));
			services.AddDistributedRedisCache(opts =>
			{
				opts.Configuration = redisConnection;
				opts.InstanceName = "App1:";
			});
			services.AddSignalR();

			var app = builder.Build();

			using (var scope = app.Services.CreateScope())
			{
				var db = scope.ServiceProvider.GetRequiredService<PanelDbContext>();
				db.Database.EnsureCreated();
			}

			if (app.Environment.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
				app.UseSwagger();
				app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ServerPanel v1"));
			}
			app.UseCors("CORSPolicy");

			//app.UseHttpsRedirection();

			app.UseRouting();

			app.UseFileServer();

			app.UseAuthentication();

			app.UseAuthorization();

			app.MapHub<ConsoleHub>("/console");
			app.MapControllers();

			app.Run();
		}
	}
}
