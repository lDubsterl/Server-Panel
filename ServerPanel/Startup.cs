using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using TokenApp;
using Newtonsoft.Json;
namespace ServerPanel
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
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
			services.AddAuthorization(options =>
			{
				options.DefaultPolicy = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
				.RequireAuthenticatedUser()
				.Build();
			});
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
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
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

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapHub<ConsoleHub>("/console");
				endpoints.MapControllers();
			});
		}
	}
}
