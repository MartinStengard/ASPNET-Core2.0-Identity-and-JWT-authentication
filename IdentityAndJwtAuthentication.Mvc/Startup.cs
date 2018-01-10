using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using IdentityAndJwtAuthentication.Data;
using IdentityAndJwtAuthentication.Models;
using IdentityAndJwtAuthentication.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.Logging;
using System.IO;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace IdentityAndJwtAuthentication
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
			// Add database connection.
			// Replace statement is used to target attached database in wwwroot/app_data folder.
			services.AddDbContext<ApplicationDbContext>(options =>
					options.UseSqlServer(
						Configuration.GetConnectionString("DefaultConnection").Replace("|DataDirectory|", Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "app_data"))));

			// Add Core Identity EF.
			services.AddIdentity<User, IdentityRole>(options =>
			{
				// Password settings
				options.Password.RequireDigit = true;
				options.Password.RequiredLength = 8;
				options.Password.RequireNonAlphanumeric = true;
				options.Password.RequireUppercase = true;
				options.Password.RequireLowercase = true;
				options.Password.RequiredUniqueChars = 2;
			})
			.AddEntityFrameworkStores<ApplicationDbContext>()
			.AddDefaultTokenProviders();

			// Add Cookie and Json Web Token Authentication.
			services.AddAuthentication()
			.AddCookie()
			.AddJwtBearer(cfg =>
			{
				cfg.RequireHttpsMetadata = false;
				cfg.SaveToken = true;
				cfg.TokenValidationParameters = new TokenValidationParameters
				{
					ValidIssuer = Configuration["BearerTokens:Issuer"],       // site that makes the token
					ValidAudience = Configuration["BearerTokens:Audience"],   // site that consumes the token
					IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["BearerTokens:Key"])),
					ValidateIssuerSigningKey = true,													// verify signature to avoid tampering
					ValidateLifetime = true,																	// validate the expiration
					ClockSkew = TimeSpan.Zero,                                // tolerance for the expiration date
					ValidateIssuer = false,                                   // TODO: change this to avoid forwarding attacks
					ValidateAudience = false                                  // TODO: change this to avoid forwarding attacks
				};
				cfg.Events = new JwtBearerEvents
				{
					OnAuthenticationFailed = context =>
					{
						var logger = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(JwtBearerEvents));
						logger.LogError("Authentication failed.", context.Exception);
						return Task.CompletedTask;
					},
					OnChallenge = context =>
					{
						var logger = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(JwtBearerEvents));
						logger.LogError("OnChallenge error", context.Error, context.ErrorDescription);
						return Task.CompletedTask;
					}
				};
			});

			// Add Policy to use expose Jwt bearer Authentication - [Authorize(Policy = "Jwt")]
			services.AddAuthorization(options =>
			{
				options.AddPolicy("Jwt", policy =>
				{
					policy.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
					policy.RequireAuthenticatedUser();
				});
			});

			// Add Cors policy to "open" access to specific websites other than this. 
			// This can be set per Action, Controller or Globally.
			// WithOrigins take one or more URLs separated by comma. The URLs must be specified without a trailing slash (/).
			services.AddCors(options =>
			{
				options.AddPolicy("CorsPolicy",
					builder => builder
							.WithOrigins(Configuration["CorsPolicy:Origins"])
							.AllowAnyMethod()
							.AllowAnyHeader()
							.AllowCredentials());
			});


			// Add application services.
			services.AddTransient<DataSeeder>();
			services.AddTransient<IEmailSender, EmailSender>();

			services.AddMvc();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env, DataSeeder seeder)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
				app.UseBrowserLink();
				app.UseDatabaseErrorPage();
			}
			else
			{
				app.UseExceptionHandler("/Home/Error");
			}

			app.UseStatusCodePages();
			app.UseStaticFiles();
			app.UseAuthentication();

			app.UseMvc(routes =>
			{
				routes.MapRoute(
									name: "default",
									template: "{controller=Home}/{action=Index}/{id?}");
			});

			// Create default data.
			seeder.SeedAsync().Wait();
		}
	}
}
