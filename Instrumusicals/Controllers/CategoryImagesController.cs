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
    public class CategoryImagesController : Controller
    {
        private readonly InstrumusicalsContext _context;

        public CategoryImagesController(InstrumusicalsContext context)
        {
            _context = context;
        }

        // GET: CategoryImages
        public async Task<IActionResult> Index()
        {
            var instrumusicalsContext = _context.CategoryImage.Include(c => c.Category);
            return View(await instrumusicalsContext.ToListAsync());
        }

        // GET: CategoryImages/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return RedirectToMalfunction();
            }

            var categoryImage = await _context.CategoryImage
                .Include(c => c.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (categoryImage == null)
            {
                return RedirectToMalfunction();
            }

            return View(categoryImage);
        }

        // GET: CategoryImages/Create
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Category.Where(c => c.CategoryImage.Image == null), nameof(Category.Id), nameof(Category.Name));
            return View();
        }

        // POST: CategoryImages/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,CategoryId,ImageFile")] CategoryImage categoryImage)
        {
            if (ModelState.IsValid)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    categoryImage.ImageFile.CopyTo(ms);
                    categoryImage.Image = ms.ToArray();
                }

                _context.Add(categoryImage);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Category, "Id", "Id", categoryImage.CategoryId);
            return View(categoryImage);
        }

        // GET: CategoryImages/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return RedirectToMalfunction();
            }

            var categoryImage = await _context.CategoryImage.Include(ci=> ci.Category).SingleOrDefaultAsync(ci => ci.Id == id);
            if (categoryImage == null)
            {
                return RedirectToMalfunction();
            }
            ViewData["CategoryId"] = new SelectList(_context.Category, nameof(Category.Id), nameof(Category.Name), categoryImage.CategoryId);
            return View(categoryImage);
        }

        // POST: CategoryImages/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CategoryId,ImageFile")] CategoryImage categoryImage)
        {
            if (id != categoryImage.Id)
            {
                return RedirectToMalfunction();
            }

            if (ModelState.IsValid)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    categoryImage.ImageFile.CopyTo(ms);
                    categoryImage.Image = ms.ToArray();
                }

                try
                {
                    _context.Update(categoryImage);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryImageExists(categoryImage.Id))
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
            ViewData["CategoryId"] = new SelectList(_context.Category, "Id", "Name", categoryImage.CategoryId);
            return View(categoryImage);
        }

        // GET: CategoryImages/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return RedirectToMalfunction();
            }

            var categoryImage = await _context.CategoryImage
                .Include(c => c.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (categoryImage == null)
            {
                return RedirectToMalfunction();
            }

            return View(categoryImage);
        }

        // POST: CategoryImages/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var categoryImage = await _context.CategoryImage.FindAsync(id);
            _context.CategoryImage.Remove(categoryImage);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CategoryImageExists(int id)
        {
            return _context.CategoryImage.Any(e => e.Id == id);
        }

        private IActionResult RedirectToMalfunction()
        {
            return RedirectToAction("Malfunction", "Home");
        }
    }
}
