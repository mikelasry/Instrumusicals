using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Instrumusicals.Data;
using Instrumusicals.Models;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;

namespace Instrumusicals.Controllers
{
    public class UsersController : Controller
    {
        private readonly InstrumusicalsContext _context;
        private List<string> adminsEmails;
        private string ERR, CREDS_ERR, USERNAME, NAVA_UN_ERR;


        public UsersController(InstrumusicalsContext context)
        {
            _context = context;

            ERR       = "Error";
            USERNAME  = "username";
            CREDS_ERR = "Credentials mismatch. Please try again";
            NAVA_UN_ERR = "Email already in use. If you forgot your password, IT'S YOUR PROBLEM.";

            adminsEmails = new List<string>();
            adminsEmails.Add("mikelasry123@gmail.com");
            adminsEmails.Add("shirboxer@gmail.com");
            adminsEmails.Add("dshmirer@gmail.com");
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.User.ToListAsync());
        }

        // GET: Users/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.User
                .FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // GET: Users/Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: Users/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register([Bind("Email,FirstName,LastName,Address,Password")] User user)
        {
            if (ModelState.IsValid)
            {
                if (_context.User.FirstOrDefault(x => x.Email.ToLower() == user.Email.ToLower()) != null)
                {
                    ViewData[ERR] = NAVA_UN_ERR;
                    return View(user);
                }

                user.Salt = SecurityManager.GenerateSalt();
                user.Hash = SecurityManager.HashPassword(user.Password, user.Salt);
                user.UserType = adminsEmails.Contains(user.Email) ? 
                                    UserType.Admin : UserType.Client;

                _context.Add(user);
                await _context.SaveChangesAsync();
                logUser(user);
                return RedirectToAction(nameof(Login));
            }
            return View(user);
        }

        // GET: Users/Login
        public IActionResult Login()
        {
            if (HttpContext.Session.GetString(USERNAME) != null)
                return RedirectToAction(nameof(Profile));
            
            return View();
        }

        // POST: Users/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login([Bind("Id,Email,Password")] User user)
        {
            User userFromDB = await _context.User.FirstOrDefaultAsync(u => u.Email.Trim().ToLower() == user.Email.Trim().ToLower());
            if (userFromDB == null)
            {
                ViewData[ERR] = CREDS_ERR;
                return View(user);
            }


            if (!SecurityManager.Validate(userFromDB, user.Password))
            {
                ViewData[ERR] = CREDS_ERR;
                return View(user);
            }

            logUser(userFromDB);
            return RedirectToAction(nameof(Profile), new { id = userFromDB.Id });
         }

        private async void logUser(User user)
        {
            if ( user == null )
            {
                //HttpContext.Session.Clear();
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return;
            }

            // HttpContext.Session.SetString(USERNAME, user.Email);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.Role, user.UserType.ToString())
            };

            var claimsIdentity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                // ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(10);
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);
        }

        public IActionResult Logout()
        {
            logUser(null);
            return RedirectToAction(nameof(Login));
        }

        // GET: Users/Profile
        [Authorize]
        public async Task<IActionResult> Profile(int? id)
          {
            string cookieIdentifier = HttpContext.User.Identity.Name;
            User user = (id == null) ?
                (await _context.User.FirstOrDefaultAsync(u => u.Email == cookieIdentifier)) :
                    (await _context.User.FirstOrDefaultAsync(u => u.Id == id));

            return (user == null) ? NotFound(user) : View(user);
        }

        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.User.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        // POST: Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Email,FirstName,LastName,Hash,Salt")] User user)
        {
            if (id != user.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(user);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        // GET: Users/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.User
                .FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.User.FindAsync(id);
            _context.User.Remove(user);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UserExists(int id)
        {
            return _context.User.Any(e => e.Id == id);
        }
    }
}
