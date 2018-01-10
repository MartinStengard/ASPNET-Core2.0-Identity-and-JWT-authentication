using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using IdentityAndJwtAuthentication.Models;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Authorization;

namespace IdentityAndJwtAuthentication.Controllers
{
	[EnableCors("CorsPolicy")]
	public class TestController : Controller
	{
		[Authorize]
		public IActionResult IsCookieAuthenticated()
		{
			return Ok("User is authenticated with Cookie");
		}

		[AllowAnonymous]
		public IActionResult IsAnonymous()
		{
			return Ok("No need for authentication for this Action");
		}

		[Authorize(Policy = "Jwt")]
		public IActionResult IsJwtAuthenticated()
		{
			return Ok("User is authenticated with Jwt Bearer");
		}

		[Authorize(Roles = "Admin")]
		public IActionResult IsAdmin()
		{
			return Ok("User is member of Admin role");
		}

		public IActionResult Error()
		{
			ViewData["Title"] = "Error";
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}
