using Instrumusicals.Data;
using Instrumusicals.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Instrumusicals.Controllers
{
    public class CartItem
    {
        public int Id { get; set; }
        public Instrument Inst { get; set; }
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

        /* @@ @@@@@@@@@@@@@@@@@@ CRUD @@@@@@@@@@@@@@@@@@ @@ */

        // @@ -- Create -- @@ //
        [AllowAnonymous]
        public IActionResult Register()
        {
            ViewData["Areas"] = GetDirectionsSelectList();
            return View();
        }
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register([Bind("Email,FirstName,LastName,Address,Password")] User user, String area, String cPassword)
        {
            if (ModelState.IsValid)
            {
                ViewData["Areas"] = GetDirectionsSelectList();
                if (!cPassword.Equals(user.Password))
                {
                    ViewData["PwMatchErr"] = "Passwords mismatch. Please try again!";
                    return View(user);
                }
                user.Email = user.Email.Trim().ToLower(); ;
                if (_context.User.FirstOrDefault(x => x.Email == user.Email) != null)
                {
                    ViewData[ERR] = NA_UN_ERR;
                    return View(user);
                }

                user.Address += GetDirectionSuffix(area);

                user.Salt = SecurityManager.GenerateSalt();
                user.Hash = SecurityManager.HashPassword(user.Password, user.Salt);
                user.UserType = adminsEmails.Contains(user.Email) ? UserType.Admin : UserType.Client;

                _context.Add(user);
                await _context.SaveChangesAsync();
                logUser(user);
                return RedirectToAction(nameof(Profile));
            }
            return View(user);
        }

        // @@ -- Read -- @@ //
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            return View(await _context.User
                .Include(u => u.Reviews)
                .Include(u => u.Orders)
                .ToListAsync());
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
            var user = await _context.User
                .Include(u => u.Orders)
                .Include(u => u.Reviews)
                .FirstOrDefaultAsync(m => m.Id == id);
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

            var user = await _context.User
                .Include(u => u.Orders)
                .Include(u => u.Reviews)
                .FirstOrDefaultAsync(m => m.Id == id);
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
            return RedirectToAction(nameof(Login));
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
            if (id == 0) id = GetAuthUserId();
            User user = await _context.User
                .Include(u => u.Orders)
                .Include(u => u.Reviews).ThenInclude(r => r.Instrument)
                .FirstOrDefaultAsync(u => u.Id == id);

            return (user == null) ? RedirectToMalfunction() : View(user);
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(int uid, string cpw, string npw)
        {
            if (uid == 0) return JsonSuccess(false, new { msg = "m" }); // -m-alfunction
            if (!IsUserAuthorized(uid)) return JsonSuccess(false, new { msg = "a" }); // -a-ccess denied

            User dbUser = await _context.User.Where(u => u.Id == uid).FirstOrDefaultAsync();
            if (dbUser == null) return JsonSuccess(false, new { msg = "m" }); // -m-alfunction

            if (!SecurityManager.Validate(dbUser, cpw)) return JsonSuccess(false, new { msg = "w" }); // -w-rong password
            dbUser.Salt = SecurityManager.GenerateSalt();
            dbUser.Hash = SecurityManager.HashPassword(npw, dbUser.Salt);

            try
            {
                _context.Update(dbUser);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            { return JsonSuccess(false, new { msg = "e" }); } // -e-xception

            return JsonSuccess(true, new { msg = "s" }); // -s-uccess
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

            var join1Query = from user in _context.User
                             join order in _context.Order on user.Id equals order.UserId
                             where user.Id == order.UserId
                             orderby order.Shipping descending
                             select new userOrderModel(user.FirstName, user.LastName, order.TotalPrice, order.Shipping);

            ViewBag.items = join1Query.ToListAsync().Result;


            var join2Query = from review in _context.Review
                             join inst in _context.Instrument on review.InstrumentId equals inst.Id
                             join catg in _context.Category on inst.CategoryId equals catg.Id
                             select new { review.Id, catg.Name };

            Dictionary<string, int> counter_ = new();
            foreach (var entry in join2Query)
            {
                if (counter_.Keys.Contains(entry.Name))
                    counter_[entry.Name]++;
                else
                    counter_.Add(entry.Name, 1);
            }
            ViewBag.counter_ = counter_;
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
            return IsAuthUserAdmin() || (GetAuthUserId() == uid);
        }

        private bool IsAuthUserAdmin()
        {
            if (!IsUserAuthenticated()) return false;
            return HttpContext.User.IsInRole("Admin");
        }

        private int GetAuthUserId()
        {
            if (!IsUserAuthenticated())
                return 0;
            return Int32.
                Parse(
                    HttpContext.User.Claims
                        .Where(claim => claim.Type == "Uid")
                        .Select(claim => claim.Value)
                        .SingleOrDefault()
                        );
        }

        private bool IsUserAuthenticated()
        {
            return HttpContext.User != null && HttpContext.User.Identity != null;
        }

        private IActionResult JsonSuccess(bool success, Object dataDict)
        {
            return Json(new { success = success, data = dataDict });

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

        private SelectList GetDirectionsSelectList(){
            return new SelectList(new[] {
                        new SelectListItem{Selected = true, Text =  "Center", Value = "c"},
                        new SelectListItem{Selected = false, Text = "North", Value = "n"},
                        new SelectListItem{Selected = false, Text = "East", Value = "e"},
                        new SelectListItem{Selected = false, Text = "South", Value = "s"},
                        new SelectListItem{Selected = false, Text = "West", Value = "w"}
                    }, "Value", "Text");
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
