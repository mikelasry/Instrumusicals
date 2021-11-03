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

namespace Instrumusicals.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CategoryImagesController : Controller
    {
        private readonly InstrumusicalsContext _context;

        public CategoryImagesController(InstrumusicalsContext context)
        {
            _context = context;
        }

        // @@ @@@@@@@@@@@@@@@@@@@@ CRUD @@@@@@@@@@@@@@@@@@@@ @@ //

        // @@ -- Create -- @@ //
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Category.Where(c => c.CategoryImage.Image == null), nameof(Category.Id), nameof(Category.Name));
            return View();
        }
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
        
        // @@ -- Read -- @@ //
        public async Task<IActionResult> Index()
        {
            var instrumusicalsContext = _context.CategoryImage.Include(c => c.Category);
            return View(await instrumusicalsContext.ToListAsync());
        }
                
        // @@ -- Update -- @@ //
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
                    { return RedirectToMalfunction(); } else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Category, "Id", "Name", categoryImage.CategoryId);
            return View(categoryImage);
        }

        // @@ -- Delete -- @@ //
        public async Task<IActionResult> Delete(int id)
        {
            if (id == 0) return RedirectToMalfunction();
            CategoryImage categoryImage = await _context.CategoryImage
                .Include(c => c.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (categoryImage == null) return RedirectToMalfunction();
            return View(categoryImage);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var categoryImage = await _context.CategoryImage.FindAsync(id);
            _context.CategoryImage.Remove(categoryImage);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // @@ -- Details -- @@ //
        public async Task<IActionResult> Details(int id)
        {
            if (id == 0) return RedirectToMalfunction();
            CategoryImage categoryImage = await _context.CategoryImage
                .Include(c => c.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (categoryImage == null) return RedirectToMalfunction();
            return View(categoryImage);
        }

        // @@ @@@@@@@@@@@@@@@@@@@@ Util functions @@@@@@@@@@@@@@@@@@@@ @@ //
        
        private bool CategoryImageExists(int id)
        {
            return _context.CategoryImage.Any(e => e.Id == id);
        }

        private bool IsUserAuthorized(int uid)
        {
            return HttpContext.User.IsInRole("Admin") || (Int32.Parse(HttpContext.User.Claims.Where(c => c.Type == "Uid").Select(c => c.Value).SingleOrDefault()) == uid);
        }

        private int GetAuthUserId()
        {
            if (HttpContext.User == null || HttpContext.User.Identity == null) return 0;
            return Int32.Parse(HttpContext.User.Claims.Where(c => c.Type == "Uid").Select(c => c.Value).SingleOrDefault());
        }

        private IActionResult JsonSuccess(bool success, Object dataDict)
        {
            return Json(new { success = success, data = dataDict });
        }



        // @@ @@@@@@@@@@@@@@@@@@@@ Reditection functions @@@@@@@@@@@@@@@@@@@@ @@ //

        [AllowAnonymous]
        private IActionResult RedirectToMalfunction()
        {
            return RedirectToAction("Malfunction", "Home");
        }

        [AllowAnonymous]
        private IActionResult RedirectToAccessDenied()
        {
            return RedirectToAction("AccessDenied", "Users");
        }
    }
}
