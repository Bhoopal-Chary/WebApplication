using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using WebApp.Data;
using WebApp.Models;
using WebApp.ViewModels;

namespace WebApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _db;

        public AccountController(ApplicationDbContext db)
        {
            _db = db;
        }

        // Hashing the password
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
        [HttpGet]
        public IActionResult Register()
        {
            var roles = _db.Roles.ToList();
            ViewBag.Roles = new SelectList(roles, "RoleId", "RoleName");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new Register
                {
                    Username = model.Username,
                    Password = HashPassword(model.Password), // Hash the password
                    Email = model.Email
                };

                _db.Registers.Add(user);
                await _db.SaveChangesAsync();

                var userRole = new EmployeeExRef
                {
                    EmployeeId = user.EmployeeId,
                    RoleId = model.SelectedRoleId
                };

                _db.EmployeeExRefs.Add(userRole);
                await _db.SaveChangesAsync();

                return RedirectToAction("LoginPage", "Login");
            }
            return View(model);
        }

        public IActionResult EmployeeDetails()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            ViewBag.UserRole = userRole;
            List<Register> registers = _db.Registers.ToList();
            return View(registers);
        }

        // VerifyPassword method implementation

        public IActionResult AdminEdit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var register = _db.Registers.Find(id);
            if (register == null)
            {
                return NotFound();
            }
            
            return View(register);

        }

        [HttpPost]
        public IActionResult AdminEdit(int? id, RegisterViewModel model)
        {
            if (id == null || !ModelState.IsValid)
            {
                return View(model);
            }

            // Fetch the existing entity from the database
            var register = _db.Registers.Find(id);
            if (register == null)
            {
                return NotFound();
            }

            // Update the properties that are allowed to be changed
            register.Password = HashPassword(model.Password);
            register.Email = model.Email;
            // Ensure Username is preserved or updated if required
            register.Username = model.Username ?? register.Username;

            _db.Registers.Update(register);
            _db.SaveChanges();
            TempData["Success"] = "Record Updated successfully";
            return RedirectToAction("EmployeeDetails");
        }
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var register = _db.Registers.Find(id);
            if (register == null)
            {
                return NotFound();
            }
            //return RedirectToAction("EmployeeDetails");
            return View(register);

        }

        [HttpPost, ActionName("Delete")]
		public IActionResult Deletepost(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var register = _db.Registers.Find(id);
			if (register == null)
			{
				return NotFound();
			}

			// Find and remove related EmployeeExRefs first
			var employeeExRefs = _db.EmployeeExRefs.Where(e => e.EmployeeId == id).ToList();
			if (employeeExRefs.Any())
			{
				_db.EmployeeExRefs.RemoveRange(employeeExRefs);
			}

			// Remove the Register record
			_db.Registers.Remove(register);
			_db.SaveChanges(true);
			TempData["Success"] = "Record Deleted successfully";
			return RedirectToAction("EmployeeDetails");
		}
	}
}

