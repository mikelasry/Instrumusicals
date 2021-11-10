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
using System.Text.Json;
using System.Net;
using System.Xml.Linq;

namespace Instrumusicals.Controllers
{
    public class StoresController : Controller
    {
        private readonly InstrumusicalsContext _context;

        public StoresController(InstrumusicalsContext context)
        {
            _context = context;
        }

   
    /* @@ @@@@@@@@@@@@@@@@@@@@ CRUD @@@@@@@@@@@@@@@@@@@@ @@ */   
        
    // @@ -- Create -- @@ //
        [Authorize(Roles = "Admin")]
        public IActionResult Create() { return View(); }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Address,Lat,Lng")] Store store)
        {
            if (ModelState.IsValid)
            {
                _context.Add(store);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(store);
        }

        // @@ -- Read -- @@ //
        public async Task<IActionResult> Index()
        {
            List<Store> stores = _context.Store.ToList();
            List<List<double>> geolocations = new();
            stores.ForEach((st =>
            {
                string requestUri = string.Format("https://maps.googleapis.com/maps/api/geocode/xml?key={1}&address={0}&sensor=false", Uri.EscapeDataString(st.Address), "AIzaSyBAAgBPty6vBndlH-lOqUDLq1cfPeDpuFI");
                WebRequest request = WebRequest.Create(requestUri);
                WebResponse response = request.GetResponse();
                XDocument xdoc = XDocument.Load(response.GetResponseStream());

                XElement result = xdoc.Element("GeocodeResponse").Element("result");
                XElement locationElement = result.Element("geometry").Element("location");
                XElement lat = locationElement.Element("lat");
                XElement lng = locationElement.Element("lng");

                geolocations.Add(new List<double> { Convert.ToDouble(lng.Value), Convert.ToDouble(lat.Value) });

            }
            ));

            ViewData["Geolocations"] = JsonSerializer.Serialize(geolocations);//Json(geolocations.ToArray()).Value.ToString();
            ViewData["Stores"] = JsonSerializer.Serialize(stores.ToArray().Select(st => st.Name).ToArray());
            return View(await _context.Store.ToListAsync());
        }

        // @@ -- Update -- @@ //
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            if (id == 0) return RedirectToMalfunction();
            var store = await _context.Store.FindAsync(id);
            if (store == null) return RedirectToMalfunction();            
            return View(store);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Address,Lat,Lng")] Store store)
        {
            if (id != store.Id) return RedirectToMalfunction();
            
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(store);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StoreExists(store.Id)) return RedirectToMalfunction();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(store);
        }


        // @@ -- Delete -- @@ //
        [Authorize]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            if (id == 0) return RedirectToMalfunction();

            Store store = await _context.Store.FirstOrDefaultAsync(m => m.Id == id);
            if (store == null) return RedirectToMalfunction();
            return View(store);
        }


        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var store = await _context.Store.FindAsync(id);
            _context.Store.Remove(store);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

    // @@ -- Details -- @@ //
        public async Task<IActionResult> Details(int id, string name)
        {
            if (id == 0 && String.IsNullOrEmpty(name)) return RedirectToMalfunction();
            if (id == 0) id = await _context.Store.Where(s => s.Name.Contains(name)).Select(s => s.Id).FirstOrDefaultAsync();
            var store = await _context.Store.FirstOrDefaultAsync(m => m.Id == id);
            if (store == null)return RedirectToMalfunction();
            return View(store);
        }

   
    // @@ @@@@@@@@@@@@@@@@@@@@ Util functions @@@@@@@@@@@@@@@@@@@@ @@ //   

        private bool StoreExists(int id)
        {
            return _context.Store.Any(e => e.Id == id);
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
