using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Instrumusicals.Data;
using Instrumusicals.Models;
using System.IO;
using Microsoft.AspNetCore.Authorization;
using System.Globalization;

namespace Instrumusicals.Controllers
{
    public class UpdateCartResult
    {
        public bool success { get; set; }
        public string msg { get; set; }
        public Object data { get; set; }
        public UpdateCartResult(bool success = false, string msg = null, Object data = null)
        {
            this.success = success;
            this.msg = msg;
            this.data = data;
        }
    }
    public class InstrumentsController : Controller
    {
        private readonly InstrumusicalsContext _context;

        public InstrumentsController(InstrumusicalsContext context)
        {
            _context = context;
        }

        /* @@ @@@@@@@@@@@@@@@@@@@@ CRUD @@@@@@@@@@@@@@@@@@@@ @@ */

        // @@ -- Create -- @@ //
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Category, nameof(Category.Id), nameof(Category.Name));
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("Id,Name,Brand,CategoryId,ImageFile,Description,Quantity,Price")] Instrument instrument)
        {
            if (ModelState.IsValid)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    if (instrument.ImageFile != null)
                    {
                        instrument.ImageFile.CopyTo(ms);
                        instrument.Image = ms.ToArray();
                    }
                }
                if (!String.IsNullOrEmpty(instrument.Brand))
                {
                    TextInfo ti = new CultureInfo("en-US", false).TextInfo;
                    instrument.Brand = ti.ToTitleCase(instrument.Brand.ToLower());
                }
                
                instrument.Sold = 0;

                _context.Add(instrument);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Category, nameof(Category.Id), nameof(Category.Name), instrument.CategoryId);
            return View(instrument);
        }

        // @@ -- Read -- @@ //
        public async Task<IActionResult> Index()
        {
            List<string> categories = new();
            categories.Add("All Categories");
            foreach (string category in await _context.Category.Select(c => c.Name).Distinct().ToListAsync())
            { categories.Add(category); }
            
            SelectList slCategories= new(categories);
            ViewData["Categories"] = slCategories;

            List<string> brands = new();
            brands.Add("All Brands");

            List<string> dbBrands = await _context.Instrument
                .Where(i => !String.IsNullOrEmpty(i.Brand))
                .Select(i => i.Brand)
                .Distinct().ToListAsync();

            foreach (string brand in dbBrands)
            { 
                brands.Add(brand); 
            }

            SelectList slBrands = new(brands);
            ViewData["Brands"] = slBrands;

            
            List<Instrument> allInstruments = await _context.Instrument.Include(i => i.Category).ToListAsync();
            if (allInstruments == null) return RedirectToMalfunction();
        
            return View(allInstruments);
        }

        // @@ -- Update -- @@ //
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return RedirectToMalfunction();

            var instrument = await _context.Instrument.FindAsync(id);
            if (instrument == null) return RedirectToMalfunction();

            ViewData["CategoryId"] = new SelectList(_context.Category, nameof(Category.Id), nameof(Category.Name), instrument.CategoryId);
            return View(instrument);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Brand,CategoryId,ImageFile,Description,Sold,Quantity,Price")] Instrument instrument)
        {
            if (id != instrument.Id) return RedirectToMalfunction();

            if (ModelState.IsValid)
            {
                if (instrument.ImageFile != null)
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        instrument.ImageFile.CopyTo(ms);
                        instrument.Image = ms.ToArray();
                    }
                }
                try
                {
                    if (instrument.Image == null)
                    {
                        byte[] image = await _context.Instrument
                            .Where(i => i.Id == id)
                            .Select(i => i.Image)
                            .SingleOrDefaultAsync();
                        if (image == null) return RedirectToMalfunction();
                        instrument.Image = image;
                    }
                    _context.Update(instrument);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!InstrumentExists(instrument.Id))
                        return RedirectToMalfunction();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Category, "Id", "Id", instrument.CategoryId);
            return View(instrument);
        }

        // @@ -- Delete -- @@ //
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return RedirectToMalfunction();

            var instrument = await _context.Instrument
                .Include(i => i.Category)
                .Include(i => i.Reviews)
                .Include(i => i.Orders)                
                .FirstOrDefaultAsync(m => m.Id == id);
            if (instrument == null) return RedirectToMalfunction();

            return View(instrument);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var instrument = await _context.Instrument.FindAsync(id);
            _context.Instrument.Remove(instrument);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // @@ -- Details -- @@ //
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return RedirectToMalfunction();

            var instrument = await _context.Instrument.Include(i => i.Reviews)
                                .Include(i => i.Category).FirstOrDefaultAsync(m => m.Id == id);
            if (instrument == null) return RedirectToAction("Malfunction", "Home");

            IEnumerable<Review> reviews = await _context.Review.Include(r => r.User)
                                .Where(r => r.InstrumentId == id).OrderByDescending(r => r.Created).ToListAsync();
            ViewData["Reviews"] = reviews;
            if (HttpContext.User != null && HttpContext.User.Identity != null)
            {
                User u = await _context.User.Where(u => u.Email == HttpContext.User.Identity.Name).FirstOrDefaultAsync();
                ViewData["UserId"] = u != null ? u.Id : u;
            }

            return View(instrument);
        }

        /* @@ @@@@@@@@@@@@@@@@@@@@ Cart @@@@@@@@@@@@@@@@@@@@ @@ */

        // -- Cart Add -- //
        [Authorize]
        public async Task<IActionResult> AddToCart(int instrumentId, int userId)
        {
            if (instrumentId == 0 || userId == 0) return JsonSuccess(false, new { msg = "d" });
            if (!IsUserAuthorized(userId)) return RedirectToAccessDenied();

            User user = await _context.User.Where(u => u.Id == userId).SingleOrDefaultAsync();
            if (user == null) return RedirectToMalfunction();
            if (user.InstrumentsWishlist == null) user.InstrumentsWishlist = new String("");

            Instrument instrumentFromDB = await _context.Instrument.Where(i => i.Id == instrumentId).SingleOrDefaultAsync();
            if (instrumentFromDB == null) return RedirectToMalfunction();
            if (instrumentFromDB.Quantity < 1) return JsonSuccess(false, new { msg = "NAV", inst = instrumentFromDB }); // Not available

            UpdateCartResult appendResult = AppaendToWishlist(ref user, instrumentFromDB);
            if (!appendResult.success) return JsonSuccess(appendResult.success, new { msg = appendResult.msg, inst = instrumentFromDB });

            try
            {
                _context.Update(user);
                _context.Update(instrumentFromDB);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            { return RedirectToMalfunction(); }

            Dictionary<int, int> id_count_dict = (Dictionary<int, int>)appendResult.data;

            List<int> leftInstrumentsIds = new();
            foreach (KeyValuePair<int, int> id_count_pair in id_count_dict) { leftInstrumentsIds.Add(id_count_pair.Key); }

            List<Instrument> leftInstruments = await _context.Instrument.Where(i => leftInstrumentsIds.Contains(i.Id)).ToListAsync();
            if (leftInstruments == null) return RedirectToMalfunction();

            List<CartItem> newCartItems = new();
            foreach (Instrument i in leftInstruments) { newCartItems.Add(new CartItem(i.Id, i, id_count_dict[i.Id])); }

            return JsonSuccess(true, new { msg = appendResult.msg, uid = userId, insts = newCartItems, inst = instrumentFromDB });
        }
        private UpdateCartResult AppaendToWishlist(ref User user, Instrument instrument)
        {
            if (User == null || instrument == null) return new UpdateCartResult(msg: "d"); // -d-efinition err

            UpdateCartResult result = new();
            Dictionary<int, int> id_count_dict = new();
            bool found = false;

            if (user.InstrumentsWishlist != "")
            { // check if the instrument already exist in wish list
                string[] inst_count_pairs = user.InstrumentsWishlist.Split(";");

                user.InstrumentsWishlist = "";

                foreach (string ic_pair in inst_count_pairs)
                {
                    try
                    {
                        if (ic_pair == "") continue;
                        int i = Int32.Parse(ic_pair.Split(",")[0]);
                        int c = Int32.Parse(ic_pair.Split(",")[1]);
                        if (i < 1 || c < 1) return new UpdateCartResult(msg: "m"); // -f-alse, -m-alfunction

                        user.InstrumentsWishlist += i + ",";
                        if (i == instrument.Id)
                        {
                            if (instrument.Quantity == 0 || instrument.Quantity - c <= 0) return new UpdateCartResult(msg: "o"); // -o-ut of stock
                            result.msg = "f"; // -f-ound
                            found = true;
                            c++;
                        }
                        user.InstrumentsWishlist += c + ";";
                        id_count_dict.Add(i, c);
                    }
                    catch { return new UpdateCartResult(msg: "e"); } // -e-xeption
                }
            }
            if (!found) { user.InstrumentsWishlist += instrument.Id + ",1;"; }

            result.success = true;
            if (result.msg == null) result.msg = "s"; // -s-uccess
            if (result.data == null) result.data = id_count_dict;

            return result;

        }


        // -- Cart Remove -- //
        public async Task<IActionResult> RemoveFromCart(int instrumentId, int userId, bool deleteAll)
        {
            if (!IsUserAuthorized(userId)) return RedirectToAccessDenied();
            User user = await _context.User.Where(u => u.Id == userId).SingleOrDefaultAsync();
            if (user == null) return RedirectToMalfunction();

            Instrument instrument = await _context.Instrument.Where(i => i.Id == instrumentId).SingleOrDefaultAsync();
            if (instrument == null) return RedirectToMalfunction();

            UpdateCartResult removeResult = DeleteFromWishlist(ref user, instrument, deleteAll);
            if (removeResult.msg.ToCharArray()[0] == 'f') return JsonSuccess(false, new { msg = removeResult.msg.ToCharArray()[1] });

            try
            {
                _context.Update(user);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            { return RedirectToMalfunction(); }

            Dictionary<int, int> id_count_dict = (Dictionary<int, int>)removeResult.data;

            List<int> leftInstrumentsIds = new();
            foreach (KeyValuePair<int, int> id_count_pair in id_count_dict) { leftInstrumentsIds.Add(id_count_pair.Key); }

            List<Instrument> leftInstruments = await _context.Instrument.Where(i => leftInstrumentsIds.Contains(i.Id)).ToListAsync();
            if (leftInstruments == null) return RedirectToMalfunction();

            List<CartItem> newCartItems = new();
            foreach (Instrument i in leftInstruments) { newCartItems.Add(new CartItem(i.Id, i, id_count_dict[i.Id])); }


            return JsonSuccess(true, new { msg = "s", uid = userId, insts = newCartItems });
        }
        private UpdateCartResult DeleteFromWishlist(ref User user, Instrument instrument, bool all)
        {
            if (User == null || instrument == null || user.InstrumentsWishlist == "")
                return new UpdateCartResult(msg: "fd"); // -f-alse, -d-efinition err

            string[] inst_count_pairs = user.InstrumentsWishlist.Split(";");
            user.InstrumentsWishlist = "";
            Dictionary<int, int> id_count_dict = new();

            foreach (string ic_pair in inst_count_pairs)
            {
                try
                {
                    if (ic_pair == "") continue; // skip the last empty iteration
                    int i = Int32.Parse(ic_pair.Split(",")[0]); // extract insturment id
                    int c = Int32.Parse(ic_pair.Split(",")[1]); // extract insturment count
                    if (i < 1 || c < 1) return new UpdateCartResult(msg: "fp"); // -f-alse, -p-arse malfunction
                    if (i == instrument.Id) if (all || c == 1) continue; else c--;
                    user.InstrumentsWishlist += i + "," + c + ";"; // assign all other instruments
                    id_count_dict.Add(i, c);// dictionary to sign the instruments' count
                }
                catch { return new UpdateCartResult(msg: "fe"); ; } // -f-alse, -e-xception err
            }
            return new UpdateCartResult(msg: "ts", data: id_count_dict);
        }


        /* @@ @@@@@@@@@@@@@@@@@@@@ Util functions @@@@@@@@@@@@@@@@@@@@ @@ */
        
        [HttpPost]
        public async Task<IActionResult> Populate(string name, string brand, int categoryId, string description, int sold, int quantity, int price)
        {
            Instrument i = new();
            i.Name = name;
            i.Brand = brand;
            i.CategoryId = categoryId;
            i.Description = description;
            i.Quantity = quantity;
            i.Price = price;
            try
            {
                _context.Add(i);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            { return JsonSuccess(false, null); }
            return JsonSuccess(true, null);
        }

        public async Task<IActionResult> SearchJson(bool all, String name , string category, string brand, float lPrice, float uPrice)
        {
            bool isAdmin = IsAuthUserAdmin();
            List<Instrument> instruments = new();
            if (all) {
                instruments = await _context.Instrument.ToListAsync();
                return JsonSuccess(true, new { msg="s", insts = instruments, isAdmin = isAdmin }) ; 
            }

            int catId = 0;
            if(!category.Equals("All Categories"))
                catId = await _context.Category.Where(c => c.Name.Contains(category)).Select(c => c.Id).FirstOrDefaultAsync();
            instruments = await _context.Instrument
                .Where(i => !String.IsNullOrEmpty(name) ? i.Name.Contains(name) : true)
                .Where(i => catId==0 ? true: i.CategoryId == catId)
                .Where(i => !brand.Equals("All Brands") ? i.Brand.Contains(brand) : true)
                .Where(i => lPrice != -1 ? i.Price >= lPrice : true)
                .Where(i => uPrice != -1 ? i.Price <= uPrice : true)
                .ToListAsync();
            if (instruments == null) return JsonSuccess(false, new {msg="e", isAdmin = false });
            return JsonSuccess(true, new { msg = "s", insts = instruments, isAdmin = isAdmin });
        }

        private bool InstrumentExists(int id)
        {
            return _context.Instrument.Any(e => e.Id == id);
        }

        private IActionResult JsonSuccess(bool success, Object dataDict)
        {
            return Json(new { success = success, data = dataDict });
        }

        private bool IsUserAuthorized(int uid)
        {
            return Int32.Parse(HttpContext.User.Claims.Where(c => c.Type == "Uid").Select(c => c.Value).SingleOrDefault()) == uid;
        }
        private bool IsUserAuthenticated()
        {
            return HttpContext.User != null && HttpContext.User.Identity != null;
        }
        private bool IsAuthUserAdmin()
        {
            return IsUserAuthenticated() && HttpContext.User.IsInRole("Admin");
        }

        // @@ @@@@@@@@@@@@@@@@@@@@ Reditection functions @@@@@@@@@@@@@@@@@@@@ @@ //

        private IActionResult RedirectToMalfunction()
        {
            return RedirectToAction("Malfunction", "Home");
        }

        private IActionResult RedirectToAccessDenied()
        {
            return RedirectToAction("AccessDenied", "Users");
        }

    }

}
