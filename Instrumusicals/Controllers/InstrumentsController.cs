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

namespace Instrumusicals.Controllers
{
    public class InstrumentsController : Controller
    {
        private readonly InstrumusicalsContext _context;

        public InstrumentsController(InstrumusicalsContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var instrumentsContext = _context.Instrument.Include(i => i.Category);
            List<Instrument> l = null;
            try { l = await instrumentsContext.ToListAsync(); }
            catch { return RedirectToMalfunction(); }
            return View(l);
        }

        public async Task<IActionResult> SearchJson(bool all, String name)
        {

            if (all)
            {
                return Json(await _context.Instrument.ToListAsync());
            }

            var q = from instrument in _context.Instrument
                    where instrument.Name.Contains(name)
                    orderby instrument.Name
                    select instrument;

            return Json(await q.ToListAsync());
        }


        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return RedirectToMalfunction();

            var instrument = await _context.Instrument.Include(i => i.Reviews)
                                .Include(i => i.Category).FirstOrDefaultAsync(m => m.Id == id);
            if (instrument == null) return RedirectToAction("Malfunction", "Home");

            IEnumerable<Review> reviews = await _context.Review.Include(r => r.User)
                                .Where(r => r.InstrumentId == id).OrderByDescending(r => r.LastUpdate).ToListAsync();
            ViewData["Reviews"] = reviews;
            if (HttpContext.User != null && HttpContext.User.Identity != null)
            {
                User u = await _context.User.Where(u => u.Email == HttpContext.User.Identity.Name).FirstOrDefaultAsync();
                ViewData["UserId"] = u != null ? u.Id : u;
            }

            return View(instrument);
        }

        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Category, nameof(Category.Id), nameof(Category.Name));
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
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

                instrument.Sold = 0;

                _context.Add(instrument);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Category, nameof(Category.Id), nameof(Category.Name), instrument.CategoryId);
            return View(instrument);
        }

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
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Brand,CategoryId,ImageFile,Description,Quantity,Price")] Instrument instrument)
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

        public async Task<IActionResult> AddToCart(int instrumentId, int userId)
        {
            if (instrumentId == 0 || userId == 0) return RedirectToMalfunction();

            User user = await _context.User.Where(u => u.Id == userId).SingleOrDefaultAsync();
            if (user == null) return RedirectToMalfunction();
            if (user.InstrumentsWishlist == null) user.InstrumentsWishlist = new String("");

            Instrument instrumentFromDB = await _context.Instrument.Where(i => i.Id == instrumentId).SingleOrDefaultAsync();
            if (instrumentFromDB == null) return RedirectToMalfunction();
            if (instrumentFromDB.Quantity < 1) return JsonSuccess(false, new { msg = "NAV", inst = instrumentFromDB });

            string appendResult = appaendToWishlist(ref user, instrumentId);
            char cause = appendResult.ToCharArray().ElementAt(1);
            if (appendResult.ToCharArray().ElementAt(0) == 'f')
            {
                if (cause == 'm' || cause == 'd' || cause == 'i' || cause == 'e')
                // malfunction / definition err / instrument err / exception
                    { return RedirectToMalfunction(); }
                return JsonSuccess(false, new { msg = cause, inst = instrumentFromDB });
            }

            try
            {
                _context.Update(user);
                _context.Update(instrumentFromDB);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            { return RedirectToMalfunction(); }

            return JsonSuccess(true, new { msg = cause, inst = instrumentFromDB });
        }

        private string appaendToWishlist(ref User user, int instrumentId)
        {
            if (User == null || instrumentId == 0) return "fd"; // -f-alse, -d-efinition err
            if (user.InstrumentsWishlist != "")
            { // check if the instrument already exist in wish list
                string[] inst_count_pairs = user.InstrumentsWishlist.Split(";");
                bool found = false;

                user.InstrumentsWishlist = "";

                foreach (string ic_pair in inst_count_pairs)
                {
                    try
                    {
                        if (ic_pair == "") continue;
                        int i = Int32.Parse(ic_pair.Split(",")[0]);
                        int c = Int32.Parse(ic_pair.Split(",")[1]);
                        if (i < 1 || c < 1) return "fm"; // -f-alse, -m-alfunction

                        user.InstrumentsWishlist += i + ",";
                        if (i == instrumentId)
                        {
                            Instrument inst =  _context.Instrument.Where(i => i.Id == instrumentId).SingleOrDefault();
                            if (inst == null) return "fi"; // -f-alse, -i-nstrument err
                            if (inst.Quantity == 0 || inst.Quantity - c <= 0) return "fo"; // -f-alse, -o-ut of stock
                            c++;
                            found = true;
                        }
                        user.InstrumentsWishlist += c + ";";
                        if (found) return "tf"; // -t-rue, -f-ound
                    }
                    catch { return "fe"; } // false, -e-xeption
                }
            }
            user.InstrumentsWishlist += instrumentId + ",1;";
            return "ts"; // -t-rue, -s-uccess
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return RedirectToMalfunction();

            var instrument = await _context.Instrument
                .Include(i => i.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (instrument == null) return RedirectToMalfunction();

            return View(instrument);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var instrument = await _context.Instrument.FindAsync(id);
            _context.Instrument.Remove(instrument);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool InstrumentExists(int id)
        {
            return _context.Instrument.Any(e => e.Id == id);
        }

        private IActionResult RedirectToMalfunction()
        {
            return RedirectToAction("Malfunction", "Home");
        }

        private IActionResult JsonSuccess(bool success, Object dataDict)
        {
            return Json(new { success = success, data = dataDict });
        }
    }
}
