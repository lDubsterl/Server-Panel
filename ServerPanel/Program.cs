using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using TokenApp;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace ServerPanel
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);
			var services = builder.Services;

			services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
					.AddJwtBearer(options =>
					{
						options.RequireHttpsMetadata = false;
						options.TokenValidationParameters = new TokenValidationParameters
						{
							// ��������, ����� �� �������������� �������� ��� ��������� ������
							ValidateIssuer = true,
							// ������, �������������� ��������
							ValidIssuer = AuthOptions.ISSUER,

							// ����� �� �������������� ����������� ������
							ValidateAudience = true,
							// ��������� ����������� ������
							ValidAudience = AuthOptions.AUDIENCE,
							// ����� �� �������������� ����� �������������
							ValidateLifetime = true,

							// ��������� ����� ������������
							IssuerSigningKey = AuthOptions.GetSymmetricSecurityKey(),
							// ��������� ����� ������������
							ValidateIssuerSigningKey = true,
						};
					});
			services.AddAuthorization();
			services.AddControllers().AddNewtonsoftJson();
			services.AddSignalR();
			services.AddCors(options =>
			{
				options.AddPolicy("CORSPolicy", builder =>
				builder.AllowAnyMethod().AllowAnyHeader().AllowCredentials().SetIsOriginAllowed((hosts) => true));
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

			var app = builder.Build();

			if (app.Environment.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
				app.UseSwagger();
				app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ServerPanel v1"));
			}

			app.UseHttpsRedirection();

			app.UseRouting();

			app.UseCors();

			app.UseFileServer();

			app.UseAuthorization();

			app.UseAuthentication();

			app.MapHub<ConsoleHub>("/console");
			app.MapControllers();

			app.Run();
		}
	}
}
