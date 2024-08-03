using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using WebApp.Data;
using WebApp.Models;
using WebApp.ViewModels;

namespace WebApp.Controllers
{
	public class LoginController : Controller
    {
        private readonly ApplicationDbContext _db;

        public LoginController(ApplicationDbContext db)
        {
            _db = db;
        }
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                var builder = new StringBuilder();
                for (var i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
        public IActionResult LoginPage()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> LoginPage(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var hasspassword = HashPassword(model.Password);
                var user = await _db.Registers
                    .FirstOrDefaultAsync(u => u.Username == model.Username && u.Password == hasspassword);
                
                if (user != null && VerifyPassword(user.Password, hasspassword))
                {
                    var userExists = new Login()
                    {
                        Username = model.Username,
                        Password = hasspassword,
                        LastLoginDate = DateTime.Now
                    };
                    _db.Logins.Add(userExists);
                    await _db.SaveChangesAsync();

                    var userRole = await _db.EmployeeExRefs
                        .Include(e => e.Role)
                        .FirstOrDefaultAsync(e => e.EmployeeId == user.EmployeeId);

                    if (userRole != null)
                    {
                        HttpContext.Session.SetString("UserRole", userRole.Role.RoleName);

                        if (userRole.Role.RoleName == "Admin")
                        {
                            return RedirectToAction("LoginActivity");
                        }
                        else if (userRole.Role.RoleName == "LoggedIn")
                        {
                            return RedirectToAction("LoginActivity");
                        }
                        else if (userRole.Role.RoleName == "ReadOnly")
                        {
                            return RedirectToAction("LoginActivity");
                        }
                    }
                }

                ModelState.AddModelError("", "Invalid login attempt.");
            }
            return View(model);
        }
		
		public IActionResult LoginActivity()
        {
			var userRole = HttpContext.Session.GetString("UserRole");
            ViewBag.UserRole = userRole;
			List<Login> objlogins = _db.Logins.ToList();
            return View(objlogins);
        }
        

        private bool VerifyPassword(string password1, string password2)
        {
            return password1.Equals(password2);
        }

	}
}
