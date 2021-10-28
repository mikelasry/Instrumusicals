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
            if ( HttpContext.User != null && HttpContext.User.Identity != null) {
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
            if (id != instrument.Id)return RedirectToMalfunction();

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
                } return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Category, "Id", "Id", instrument.CategoryId);
            return View(instrument);
        }

        public async Task<IActionResult> AddToCart(int instrumentId, int userId)
        {
            if (instrumentId == 0 || userId ==0) return RedirectToMalfunction();

            User user = await _context.User.Where(u => u.Id == userId).SingleOrDefaultAsync();
            if (user == null) return RedirectToMalfunction();
            if (user.InstrumentsWishlist == null)
                user.InstrumentsWishlist = new String("");

            user.InstrumentsWishlist += instrumentId + ",1;";
            Instrument instrumentFromDB = await _context.Instrument.Where(i => i.Id == instrumentId).SingleOrDefaultAsync();
            if (instrumentFromDB == null) return RedirectToMalfunction();

            /*try
            {
                _context.Update(user);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return RedirectToMalfunction();
            }*/

            return JsonSuccess(true, instrumentFromDB);
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
