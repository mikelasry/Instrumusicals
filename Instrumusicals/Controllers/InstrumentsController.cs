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

        // GET: Instruments
        public async Task<IActionResult> Index()
        {
            /* Get instruments WITH CATEGORIES (Include method)*/
            var instrumentsContext = _context.Instrument.Include(i => i.Category);
            return View(await instrumentsContext.ToListAsync());
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

        // GET: Instruments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var instrument = await _context.Instrument
                .Include(i => i.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (instrument == null)
            {
                return NotFound();
            }

            return View(instrument);
        }

        // GET: Instruments/Create
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Category, nameof(Category.Id), nameof(Category.Name));
            return View();
        }

        // POST: Instruments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
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

        // GET: Instruments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var instrument = await _context.Instrument.FindAsync(id);
            if (instrument == null)
            {
                return NotFound();
            }
            ViewData["CategoryId"] = new SelectList(_context.Category, nameof(Category.Id), nameof(Category.Name), instrument.CategoryId);
            return View(instrument);
        }

        // POST: Instruments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Brand,CategoryId,ImageFile,Description,Quantity,Price")] Instrument instrument)
        {
            if (id != instrument.Id)
            {
                return NotFound();
            }

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
            ViewData["CategoryId"] = new SelectList(_context.Category, "Id", "Id", instrument.CategoryId);
            return View(instrument);
        }

        // GET: Instruments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var instrument = await _context.Instrument
                .Include(i => i.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (instrument == null)
            {
                return NotFound();
            }

            return View(instrument);
        }

        // POST: Instruments/Delete/5
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
    }
}
