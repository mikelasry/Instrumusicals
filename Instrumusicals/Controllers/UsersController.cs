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
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Instrumusicals.Controllers
{
    [Authorize]
    public class UsersController : Controller
    {
        private readonly InstrumusicalsContext _context;
        private List<string> adminsEmails;
        private string ERR, CREDS_ERR, USERNAME, NAVA_UN_ERR;

        private string mikesMail, shirsMail, dansMail;

        public UsersController(InstrumusicalsContext context)
        {
            _context = context;

            ERR = "Error";
            USERNAME = "username";
            CREDS_ERR = "Credentials mismatch. Please try again";
            NAVA_UN_ERR = "Email already in use. If you forgot your password, IT'S YOUR PROBLEM.";

            mikesMail = "mikelasry123@gmail.com";
            shirsMail = "shirboxer@gmail.com";
            dansMail = "dshmirer@gmail.com";

            adminsEmails = new List<string>();
            adminsEmails.Add(mikesMail);
            adminsEmails.Add(shirsMail);
            adminsEmails.Add(dansMail);

        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.User.ToListAsync());
        }

        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Admin()
        {
            return View();
        }

        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return RedirectToMalfunction();
            }

            // TODO: check if authenticated user is authorized (self or admin)
            var user = await _context.User
                .FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return RedirectToMalfunction();
            }

            return View(user);
        }

        [AllowAnonymous]
        public IActionResult Register()
        {
            ViewData["Areas"] = new SelectList(new[] {
                        new SelectListItem{Selected = true, Text =  "Center", Value = "c"},
                        new SelectListItem{Selected = false, Text = "North", Value = "n"},
                        new SelectListItem{Selected = false, Text = "East", Value = "e"},
                        new SelectListItem{Selected = false, Text = "South", Value = "s"},
                        new SelectListItem{Selected = false, Text = "West", Value = "w"}
                    }, "Value", "Text");
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register([Bind("Email,FirstName,LastName,Address,Password")] User user, String area)
        {
            if (ModelState.IsValid)
            {
                if (_context.User.FirstOrDefault(x => x.Email.ToLower() == user.Email.ToLower()) != null)
                {
                    ViewData[ERR] = NAVA_UN_ERR;
                    return View(user);
                }

                user.Address += getDirectionSuffix(area);

                user.Salt = SecurityManager.GenerateSalt();
                user.Hash = SecurityManager.HashPassword(user.Password, user.Salt);
                user.UserType = adminsEmails.Contains(user.Email) ?
                                    UserType.Admin : UserType.Client;

                _context.Add(user);
                await _context.SaveChangesAsync();
                logUser(user);
                return RedirectToAction(nameof(Index), "Home");

            }
            return View(user);
        }

        private string getDirectionSuffix(String area)
        {
            switch (area)
            {
                case "e": return ", East";
                case "w": return ", West";
                case "s": return ", South";
                case "n": return ", North";
                case "c": return ", Center";
                default: return ", NA";
            }
        }

        [AllowAnonymous]
        public IActionResult Login()
        {
            if (!String.IsNullOrEmpty(HttpContext.User.Identity.Name))
                return RedirectToAction(nameof(Profile));

            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login([Bind("Id,Email,Password")] User user, String ReturnUrl)
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
            

            return ReturnUrl == null ? RedirectToAction(nameof(Index), "Home") : LocalRedirect(ReturnUrl);
        }

        private async void logUser(User user)
        {
            if (user == null)
            {
                //HttpContext.Session.Clear();
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return;
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.Role, user.UserType.ToString()),
                new Claim("FullName", user.FirstName + " " + user.LastName),
                new Claim("Uid", user.Id.ToString())
                // Todo: sign to cookie users cart
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
            return RedirectToAction(nameof(Index), "Home");
        }

        public IActionResult Cart()
        {
            return View();
        }

        public async Task<IActionResult> Profile(int? id)
        {
            string cookieIdentifier = HttpContext.User.Identity.Name;
            User user = (id == null || id == 0) ?
                (await _context.User.FirstOrDefaultAsync(u => u.Email == cookieIdentifier)) :
                    (await _context.User.FirstOrDefaultAsync(u => u.Id == id));

            return (user == null) ? NotFound(user) : View(user);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return RedirectToMalfunction();
            }

            // TODO: check if authenticated user is authorized (self or admin)
            // if not so, retrun redirect AccessDenied

            var user = await _context.User.FindAsync(id);
            if (user == null)
            {
                return RedirectToMalfunction();
            }
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Email,FirstName,LastName,Hash,Salt")] User user)
        {
            if (id != user.Id)
            {
                return RedirectToMalfunction();
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
                        return RedirectToMalfunction();
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

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return RedirectToMalfunction();
            }

            // TODO: check if authenticated user is authorized (self or admin)
            // if not so, retrun redirect AccessDenied

            var user = await _context.User
                .FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return RedirectToMalfunction();
            }

            return View(user);
        }

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


        private IActionResult RedirectToMalfunction()
        {
            return RedirectToAction("Malfunction", "Home");
        }
    }
}
