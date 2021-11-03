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
using System.Collections;

namespace Instrumusicals.Controllers
{
    public class CartItem
    {
        public int Id { get; set; }
        public Instrument Inst{ get; set; }
        public int Count { get; set; }
        public CartItem(int Id, Instrument Inst, int Count)
        {
            this.Id = Id;
            this.Inst = Inst;
            this.Count = Count;
        }
    }

    [Authorize]
    public class UsersController : Controller
    {
        private readonly InstrumusicalsContext _context;
        private List<string> adminsEmails;
        private string ERR, CREDS_ERR, NA_UN_ERR;

        private string mikesMail, shirsMail, dansMail;

        // ---------------------- C'tor ------------------------- //
        public UsersController(InstrumusicalsContext context)
        {
            _context = context;

            ERR = "Error";
            CREDS_ERR = "Credentials mismatch. Please try again";
            NA_UN_ERR = "Email already in use. If you forgot your password, IT'S YOUR PROBLEM.";

            mikesMail = "mikelasry123@gmail.com";
            shirsMail = "shirboxer@gmail.com";
            dansMail = "dshmirer@gmail.com";

            adminsEmails = new List<string>();
            adminsEmails.Add(mikesMail);
            adminsEmails.Add(shirsMail);
            adminsEmails.Add(dansMail);

        }

        // @@ @@@@@@@@@@@@@@@@@@ CRUD @@@@@@@@@@@@@@@@@@ @@ //

        // @@ -- Create -- @@ //
        [AllowAnonymous] // Create: Get
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
        [ValidateAntiForgeryToken] // Create: Post
        public async Task<IActionResult> Register([Bind("Email,FirstName,LastName,Address,Password")] User user, String area)
        {
            if (ModelState.IsValid)
            {
                user.Email = user.Email.Trim().ToLower(); ;
                if (_context.User.FirstOrDefault(x => x.Email == user.Email) != null)
                {
                    ViewData[ERR] = NA_UN_ERR;
                    return View(user);
                }

                user.Address += GetDirectionSuffix(area);

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

        // @@ -- Read -- @@ //
        // Read: Get
        public async Task<IActionResult> Index()
        {
            return View(await _context.User.ToListAsync());
        }

        // @@ -- Update -- @@ //
        public async Task<IActionResult> Edit(int id)
        {
            if (id == 0) return RedirectToMalfunction();
            if (!IsUserAuthorized(id)) return RedirectToAccessDenied();
            var user = await _context.User.FindAsync(id);
            if (user == null) return RedirectToMalfunction();
            return View(user);
        }
        [HttpPost]
        [ValidateAntiForgeryToken] // Update: Post
        public async Task<IActionResult> Edit(int id, [Bind("Id,Email,FirstName,LastName,Hash,Salt")] User user)
        {
            if (id != user.Id) return RedirectToMalfunction();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(user);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!IsUserExists(user.Id)) return RedirectToMalfunction();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        // @@ -- Delete -- @@ //
        public async Task<IActionResult> Delete(int id)
        {
            if (id == 0) return RedirectToMalfunction();
            if (!IsUserAuthorized(id)) return AccessDenied();
            var user = await _context.User.FirstOrDefaultAsync(m => m.Id == id);
            if (user == null) return RedirectToMalfunction();
            return View(user);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken] // Delete: Post
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.User.FindAsync(id);
            _context.User.Remove(user);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // @@ -- Details -- @@ //
        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            if (id == 0) return RedirectToMalfunction();
            if (!IsUserAuthorized(id)) return RedirectToAccessDenied();

            var user = await _context.User.FirstOrDefaultAsync(m => m.Id == id);
            if (user == null) return RedirectToMalfunction();
            return View(user);
        }

        // @@ @@@@@@@@@@@@@@@@@@ User Auth @@@@@@@@@@@@@@@@@@ @@ //

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
            user.Email = user.Email.Trim().ToLower();
            User userFromDB = await _context.User.FirstOrDefaultAsync(u => u.Email.Trim().ToLower() == user.Email);
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
        public IActionResult Logout()
        {
            logUser(null);
            return RedirectToAction(nameof(Index), "Home");
        }
        private async void logUser(User user)
        {
            if (user == null)
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return;
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.Role, user.UserType.ToString()),
                new Claim("FullName", user.FirstName + " " + user.LastName),
                new Claim("Uid", user.Id.ToString())
            };

            var claimsIdentity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                // Cookie lifetime declared in startup file
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);
        }

        // @@ @@@@@@@@@@@@@@@@@@ Profile & Admin Panel @@@@@@@@@@@@@@@@@@ @@ //

        public async Task<IActionResult> Profile(int id)
        {
            if(id == 0) id = GetAuthUserId();
            User user = await _context.User
                .Include(u => u.Orders)
                .Include(u => u.Reviews)
                .FirstOrDefaultAsync(u => u.Id == id);

            return (user == null) ? RedirectToMalfunction() : View(user);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Admin()
        {
            Dictionary<string, int> data = new Dictionary<string, int>();
            Dictionary<string, int> data2 = new Dictionary<string, int>();

            var query_ = from row in _context.Instrument.AsEnumerable()
                         group row by row.CategoryId into groups
                         select groups;

            foreach (var groupName in query_) // for each category
            {
                var name = _context.Category.Find(groupName.Key).Name;
                Console.WriteLine(name);
                int i = 0;
                int counter = 0;
                foreach (var inst in groupName) // for each instrument
                {
                    counter++;
                    i += (((int)inst.Price) * ((int)inst.Quantity));
                }
                data2.Add(name, counter);
                data.Add(name, i);
            }

            ViewData["data"] = data;
            ViewData["data2"] = data2;

            return View();
        }

        // @@ @@@@@@@@@@@@@@@@@@ Cart @@@@@@@@@@@@@@@@@@ @@ //

        public async Task<IActionResult> Cart()
        {
            int uid = GetAuthUserId();
            if (uid < 1) RedirectToMalfunction();

            User dbUser = await _context.User.Where(u => u.Id == uid).SingleOrDefaultAsync();
            if (dbUser == null) RedirectToMalfunction();

            IDictionary<int, int> countDict = new Dictionary<int, int>();
            string[] inst_count_pairs = dbUser.InstrumentsWishlist.Split(";");
            List<int> instsIds = new();
            int i = 0;

            foreach (string inst_count_pair in inst_count_pairs)
            {
                if (inst_count_pair.Trim() == "") continue;
                int instId = Int32.Parse(inst_count_pair.Split(",")[0]);
                int instCount = Int32.Parse(inst_count_pair.Split(",")[1]);
                if (instId < 1 || instCount < 0) return RedirectToMalfunction();

                instsIds.Add(instId);
                countDict.Add(new KeyValuePair<int, int>(instId, instCount));
            }

            List<Instrument> cartInsts = await _context.Instrument.Where(i => instsIds.Contains(i.Id)).ToListAsync();
            if (cartInsts == null) return RedirectToMalfunction();

            List<CartItem> cartBag = new();
            foreach (Instrument cartItem in cartInsts)
            {
                int instCount;
                if (!countDict.TryGetValue(cartItem.Id, out instCount))
                    { return RedirectToMalfunction(); }
                cartBag.Add(new CartItem(cartItem.Id, cartItem, instCount));
            }

            ViewData["CartBag"] = cartBag;
            return View();
        }

        // @@ @@@@@@@@@@@@@@@@@@ Util functions @@@@@@@@@@@@@@@@@@ @@ //

        private bool IsUserExists(int id)
        {
            return _context.User.Any(e => e.Id == id);
        }

        private bool IsUserAuthorized(int uid)
        {
            return HttpContext.User.IsInRole("Admin") || (Int32.Parse(HttpContext.User.Claims.Where(c => c.Type == "Uid").Select(c => c.Value).SingleOrDefault()) == uid) ;
        }
        private int GetAuthUserId()
        {
            if (HttpContext.User == null || HttpContext.User.Identity == null) return 0;
            return Int32.Parse(HttpContext.User.Claims.Where(c => c.Type == "Uid").Select(c => c.Value).SingleOrDefault());
        }

        private string GetDirectionSuffix(String area)
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

        // @@ @@@@@@@@@@@@@@@@@@ Reditection functions @@@@@@@@@@@@@@@@@@ @@ //

        private IActionResult RedirectToMalfunction()
        {
            return RedirectToAction("Malfunction", "Home");
        }
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }
        private IActionResult RedirectToAccessDenied()
        {
            return RedirectToAction("AccessDenied", "Users");
        }
    }
}
