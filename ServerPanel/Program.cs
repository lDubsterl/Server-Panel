using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Panel.Application.DTOs.AuthenticationRequests;
using Panel.Infrastructure.Extensions;
using Panel.Infrastructure.Hubs;
using Panel.Infrastructure.Services;
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

			services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetAssembly(typeof(Login))));
			services.AddInfrastructureLayer(builder.Configuration);

			services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
					.AddJwtBearer(options =>
					{
						options.RequireHttpsMetadata = false;
						options.TokenValidationParameters = new TokenValidationParameters
						{
							// ��������, ����� �� �������������� �������� ��� ��������� ������
							ValidateIssuer = true,
							// ������, �������������� ��������
							ValidIssuer = TokenBuilder.Issuer,

							// ����� �� �������������� ����������� ������
							ValidateAudience = true,
							// ��������� ����������� ������
							ValidAudience = TokenBuilder.Audience,
							// ����� �� �������������� ����� �������������
							ValidateLifetime = true,

							// ��������� ����� ������������
							IssuerSigningKey = new SymmetricSecurityKey(Convert.FromBase64String(TokenBuilder.Secret)),
							// ��������� ����� ������������
							ValidateIssuerSigningKey = true,
						};
					});
			services.AddCors(options =>
			{
				options.AddPolicy("CORSPolicy", builder =>
				builder.AllowAnyMethod()
				.WithOrigins("http://localhost:3000")
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

			//var redisConnection = builder.Configuration["RedisConnection"];
			//services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConnection));
			//services.AddDistributedRedisCache(opts =>
			//{
			//	opts.Configuration = redisConnection;
			//	opts.InstanceName = "App1:";
			//});
			services.AddSignalR();

			var app = builder.Build();

			if (app.Environment.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
				app.UseSwagger();
				app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ServerPanel v1"));
			}

			app.UseHttpsRedirection();

			app.UseRouting();

			app.UseCors("CORSPolicy");

			app.UseFileServer();

			app.UseAuthentication();

			app.UseAuthorization();

			app.MapHub<ConsoleHub>("/console");
			app.MapControllers();

			app.Run();
		}
	}
}
