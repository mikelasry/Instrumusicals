﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Instrumusicals.Data;
using Instrumusicals.Models;

namespace Instrumusicals.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly InstrumusicalsContext _context;

        public CategoriesController(InstrumusicalsContext context)
        {
            _context = context;
        }

        // @@ @@@@@@@@@@@@@@@@@@@@ CRUD @@@@@@@@@@@@@@@@@@@@@ @@ //

        // @@ -- Create -- @@ //
        public IActionResult Create() { return View(); }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name")] Category category)
        {
            if (ModelState.IsValid)
            {
                _context.Add(category);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Create), "CategoryImages");
            }
            return View(category);
        }

        // @@ -- Read -- @@ //
        public async Task<IActionResult> Index()
        {
            return View(await _context.Category.Include(c => c.CategoryImage).ToListAsync());
        }

        // @@ -- Update -- @@ //
        public async Task<IActionResult> Edit(int id)
        {
            if (id == 0) return RedirectToMalfunction();
            var category = await _context.Category
                .Include(c => c.CategoryImage).FirstOrDefaultAsync(c => c.Id == id);
            return (category == null) ? RedirectToMalfunction() : View(category);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name")] Category category)
        {
            if (id != category.Id) return RedirectToMalfunction();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(category);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.Id)) 
                        return RedirectToMalfunction();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // @@ -- Delete -- @@ //
        public async Task<IActionResult> Delete(int id)
        {
            if (id == 0) return RedirectToMalfunction();
            Category category = await _context.Category
                .FirstOrDefaultAsync(m => m.Id == id);
            return (category == null) ? RedirectToMalfunction() : View(category);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            Category category = await _context.Category.FindAsync(id);
            _context.Category.Remove(category);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        
        // @@ -- Details-- @@ //
        public async Task<IActionResult> Details(int id)
            {
                if (id == 0) return RedirectToMalfunction();
                Category category = await _context.Category.Include(c => c.CategoryImage)
                    .FirstOrDefaultAsync(m => m.Id == id);
                return (category == null) ? RedirectToMalfunction() : View(category);
            }
        
        // @@ @@@@@@@@@@@@@ Util functions @@@@@@@@@@@@@@@ @@ //

        private bool CategoryExists(int id)
        {
            return _context.Category.Any(e => e.Id == id);
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


        // @@ @@@@@@@@@@@@@@@@ Reditection functions @@@@@@@@@@@@@@@@ @@ //

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

