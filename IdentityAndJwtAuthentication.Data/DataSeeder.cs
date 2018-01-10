using IdentityAndJwtAuthentication.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace IdentityAndJwtAuthentication.Data
{
	public class DataSeeder
	{
		private readonly ApplicationDbContext _context;
		private readonly UserManager<User> _userManager;

		public DataSeeder(ApplicationDbContext context, UserManager<User> userManager)
		{
			_context = context;
			_userManager = userManager;
		}

		public async Task SeedAsync()
		{
			_context.Database.EnsureCreated();

			// Add default roles if not set.
			if (!_context.Roles.Any())
			{
				_context.Add(new IdentityRole { Name = "Admin" });
				_context.Add(new IdentityRole { Name = "User" });
				_context.SaveChanges();
			}

			// Add default admin user if no users exists.
			if (!_context.Users.Any())
			{
				var adminUser = new User()
				{
					Email = "test@test.test",
					UserName = "test@test.test"
				};

				var result = await _userManager.CreateAsync(adminUser, "P@ssw0rd!");
				if (result.Succeeded)
				{
					adminUser.EmailConfirmed = true;
					await _userManager.UpdateAsync(adminUser);
				}
			}
		}
	}
}
