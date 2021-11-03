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
    [Authorize]
    public class ReviewsController : Controller
    {
        private readonly InstrumusicalsContext _context;

        public ReviewsController(InstrumusicalsContext context)
        {
            _context = context;
        }

        /* @@ @@@@@@@@@@@@@@@@@@@ CRUD @@@@@@@@@@@@@@@@@@@ @@ */

        // @@ -------------------- Create ------------------- @@ //
        public async Task<IActionResult> Create(int? inst)
        {
            ViewData["InstrumentId"] = inst;
            ViewData["Instrument"] = await _context.Instrument.Where(i => i.Id == inst).FirstOrDefaultAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,InstrumentId,Content")] Review review, int instrument)
        {
            if (ModelState.IsValid)
            {
                string cookieIdentifier = HttpContext.User.Identity.Name;
                User user = await _context.User.FirstOrDefaultAsync(u => u.Email == cookieIdentifier);
                if (user == null) return RedirectToMalfunction();

                review.Id = 0;
                review.UserId = user.Id;

                review.LastUpdate = review.Created;
                review.InstrumentId = instrument;

                _context.Add(review);
                await _context.SaveChangesAsync();
                return LocalRedirect("/Instruments/Details/" + instrument);
            }
            ViewData["Instruments"] = new SelectList(_context.Instrument, "Id", "Name", review.InstrumentId);
            return View(review);
        }

        // @@ --------------------- Read ------------------- @@ //
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var instrumusicalsContext = _context.Review
                .Include(r => r.Instrument)
                .Include(r=> r.User);
            return View(await instrumusicalsContext.ToListAsync());
        }
        
        // @@ -------------------- Update ------------------ @@ //
        public async Task<IActionResult> Edit(int id)
        {
            if (id == 0) return RedirectToMalfunction();
            Review dbReview = await _context.Review.Include(r => r.Instrument).Where(r => r.Id == id).FirstOrDefaultAsync();
            if (dbReview == null) return RedirectToMalfunction();
            if (!IsUserAuthorized(dbReview.UserId)) return RedirectToAccessDenied();
            ViewData["Instrument"] = dbReview.Instrument;
            return View(dbReview);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,InstrumentId,Created,LastUpdate,Content")] Review review, string returnUrl)
        {
            if (id != review.Id) return RedirectToMalfunction();
            Review reviewFromDb = await _context.Review.Where(r => r.Id == id).FirstOrDefaultAsync();
            if (reviewFromDb == null) return RedirectToMalfunction();

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
                { if (!ReviewExists(review.Id)) return RedirectToMalfunction(); else throw;}
                return returnUrl != null ? LocalRedirect(returnUrl) : RedirectToAction(nameof(Index), "Instruments");
            }
            return View(review);
        }

        // @@ -------------------- Delete ----------------- @@ //
        public async Task<IActionResult> Delete(int id)
        {
            if (id == 0) return RedirectToMalfunction();
            var review = await _context.Review.Include(r => r.Instrument).FirstOrDefaultAsync(m => m.Id == id);
            if (review == null) return RedirectToMalfunction();
            if (!IsUserAuthorized(review.UserId)) return RedirectToAccessDenied();
            return View(review);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (id == 0) return RedirectToMalfunction();
            var review = await _context.Review.FindAsync(id);
            if (!IsUserAuthorized(review.UserId)) return RedirectToAccessDenied();

            _context.Review.Remove(review);
            await _context.SaveChangesAsync();
            return LocalRedirect("/Instruments/Details/" + review.InstrumentId);
        }
        
        // @@ -------------------- Details----------------- @@ //
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return RedirectToMalfunction();

            var review = await _context.Review
                .Include(r => r.Instrument)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (review == null) return RedirectToMalfunction();
            return View(review);
        }

        // @@ @@@@@@@@@@@@@@@@@@@ Util functions @@@@@@@@@@@@@@@@@@@ @@ //

        private bool ReviewExists(int id)
        {
            return _context.Review.Any(e => e.Id == id);
        }
        
        private bool IsUserAuthorized(int uid)
        {
            return HttpContext.User.IsInRole("Admin") || Int32.Parse(HttpContext.User.Claims.Where(c => c.Type == "Uid").Select(c => c.Value).SingleOrDefault()) == uid;
        }

        // @@ @@@@@@@@@@@@@@ Reditection functions @@@@@@@@@@@@@@ @@ //

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
