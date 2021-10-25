using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Instrumusicals.Data;
using Instrumusicals.Models;
using Microsoft.AspNetCore.Authorization;

namespace Instrumusicals.Controllers
{
    public class ReviewsController : Controller
    {
        private readonly InstrumusicalsContext _context;

        public ReviewsController(InstrumusicalsContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var instrumusicalsContext = _context.Review
                .Include(r => r.Instrument)
                .Include(r=> r.User);
            return View(await instrumusicalsContext.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Malfunction","Home");
            }

            var review = await _context.Review
                .Include(r => r.Instrument)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (review == null)
            {
                return RedirectToAction("Malfunction","Home");
            }

            return View(review);
        }

        [Authorize]
        public async Task<IActionResult> Create(int? inst)
        {
            ViewData["InstrumentId"] = inst;
            ViewData["Instrument"] = await _context.Instrument.Where(i => i.Id == inst).FirstOrDefaultAsync();
            return View();
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,InstrumentId,Content")] Review review, int instrument)
        {
            if (ModelState.IsValid)
            {
                string cookieIdentifier = HttpContext.User.Identity.Name;
                User user = await _context.User.FirstOrDefaultAsync(u => u.Email == cookieIdentifier);
                if (user == null) return RedirectToAction("Malfunction", "Home");

                review.Id = 0;
                review.UserId = user.Id;
                review.LastUpdate = review.Created;
                review.InstrumentId = instrument;

                _context.Add(review);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["Instruments"] = new SelectList(_context.Instrument, "Id", "Name", review.InstrumentId);
            return View(review);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Malfunction","Home");
            }

            var review = await _context.Review.FindAsync(id);
            if (review == null) return RedirectToAction("Malfunction","Home");
            
            Review reviewFromDB = await _context.Review
                                            .Include(r => r.Instrument)
                                            .Where(r => r.Id == id)
                                            .FirstOrDefaultAsync();
            if (reviewFromDB == null) return RedirectToAction("Malfunction","Home");

            ViewData["Instrument"] = reviewFromDB.Instrument;
            return View(review);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,InstrumentId,Created,LastUpdate,Content")] Review review)
        {
            if (id != review.Id) return RedirectToAction("Malfunction","Home");

            Review reviewFromDb = await _context.Review.Where(r => r.Id == id).FirstOrDefaultAsync();
            if (reviewFromDb == null) return RedirectToAction("Malfunction","Home");

            reviewFromDb.Content = review.Content;
            reviewFromDb.LastUpdate = DateTime.Now;

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(reviewFromDb);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ReviewExists(review.Id))
                        return RedirectToAction("Malfunction","Home");
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(review);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Malfunction","Home");
            }

            var review = await _context.Review
                .Include(r => r.Instrument)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (review == null) return RedirectToAction("Malfunction","Home");

            return View(review);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var review = await _context.Review.FindAsync(id);
            _context.Review.Remove(review);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ReviewExists(int id)
        {
            return _context.Review.Any(e => e.Id == id);
        }
    }
}
