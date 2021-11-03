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
    public class OrdersController : Controller
    {
        private readonly InstrumusicalsContext _context;

        public OrdersController(InstrumusicalsContext context)
        {
            _context = context;
        }

        /* @@ @@@@@@@@@@@@@@@@@@@@ CRUD @@@@@@@@@@@@@@@@@@@@ @@ */

        // @@ -- Create -- @@ //
        public IActionResult Create()
        {
            ViewData["UserId"] = new SelectList(_context.Set<User>(), "Id", "Id");
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("Id,UserId,Address,TotalPrice,Create,LastUpdate")] Order order)
        {
            if (ModelState.IsValid)
            {
                order.Id = 0;
                _context.Add(order);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["UserId"] = new SelectList(_context.Set<User>(), "Id", "Id", order.UserId);
            return View(order);
        }
        
        // @@ -- Read -- @@ //
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var instrumusicalsContext = _context.Order.Include(o => o.User);
            return View(await instrumusicalsContext.ToListAsync());
        }

        // @@ -- Update -- @@ //
        public async Task<IActionResult> Edit(int id)
        {
            if (id == 0) return RedirectToMalfunction();
            Order order = await _context.Order.Where(o => o.Id == id).Include(o => o.Instruments).FirstOrDefaultAsync();
            if (order == null) return RedirectToMalfunction();
            if (!IsUserAuthorized(order.UserId)) return RedirectToAccessDenied();
            
            ViewData["UserId"] = new SelectList(_context.Set<User>(), "Id", "Id", order.UserId);
            return View(order);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,UserId,Address,TotalPrice,Create,LastUpdate")] Order order)
        {
            if (id != order.Id) return RedirectToMalfunction();
            Order dbOrder = await _context.Order.Where(o => o.Id == id).FirstOrDefaultAsync();
            if (!IsUserAuthorized(dbOrder.UserId)) return RedirectToAccessDenied();
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(order);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                { if (!OrderExists(order.Id)) return RedirectToMalfunction(); else throw; }
                
                return RedirectToAction("Profile","Users");
            }
            ViewData["UserId"] = new SelectList(_context.Set<User>(), "Id", "Id", order.UserId);
            return View(order);
        }
        
        // @@ -- Delete -- @@ //
        public async Task<IActionResult> Delete(int id)
        {
            if (id == 0) return RedirectToMalfunction();
            var order = await _context.Order.Include(o => o.User).FirstOrDefaultAsync(m => m.Id == id);
            if (order == null) return RedirectToMalfunction();
            if (!IsUserAuthorized(order.UserId)) return RedirectToAccessDenied();
            return View(order);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var order = await _context.Order.FindAsync(id);
            if (order == null) return RedirectToMalfunction();
            if (!IsUserAuthorized(order.UserId)) return RedirectToAccessDenied();
            _context.Order.Remove(order);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // @@ -- Details -- @@ //
        public async Task<IActionResult> Details(int id)
        {
            if (id == 0) return RedirectToMalfunction();
            Order order = await _context.Order.Include(o => o.User).Include(o => o.Instruments).FirstOrDefaultAsync(m => m.Id == id);
            if (order == null) return RedirectToMalfunction();
            if (!IsUserAuthorized(order.UserId)) return RedirectToAccessDenied();
            return View(order);
        }

        // @@ @@@@@@@@@@@@@@@@@@@@ Util functions @@@@@@@@@@@@@@@@@@@@ @@ //

        public async Task<IActionResult> PlaceOrder(int uid)
        {
            // -- check authorization -- //
            if (!IsUserAuthorized(uid)) // not -a-uthrorized
                return JsonSuccess(false, new { msg = "a" });

            // -- get user from db -- //
            User dbUser =  await _context.User.Where(u => u.Id == uid).SingleOrDefaultAsync();
            if(dbUser == null) return JsonSuccess(false, new { msg = "m" });

            // -- make sure wishlist is not empty -- //
            if ( String.IsNullOrEmpty(dbUser.InstrumentsWishlist) ) // -m-alfunction
                return JsonSuccess(false, new { msg = "m" }); 

            // -- init util variables -- //
            List<int> instrumentsIds = new();
            IDictionary<int, int> ic_dict = new Dictionary<int,int>();
            int i, c;

            // -- create instrumentId->count dictionary and fill it -- //
            string[] id_count_pairs = dbUser.InstrumentsWishlist.ToString().Split(";");
            foreach(string id_count_pair in id_count_pairs)
            {
                if (String.IsNullOrEmpty(id_count_pair)) continue;
                i = Int32.Parse(id_count_pair.Split(",")[0]);
                c = Int32.Parse(id_count_pair.Split(",")[1]);
                if (i < 1 || c < 1)  // -m-alfunction
                    return JsonSuccess(false, new { msg = "m" });

                instrumentsIds.Add(i);
                ic_dict.Add(new KeyValuePair<int, int>(i, c));
            } 
            if (ic_dict.Keys.Count < 1)  // -m-alfunction
                return JsonSuccess(false, new { msg = "m" });

            // -- get instruments from db -- //
            List<Instrument> dbInsts = await _context.Instrument
                .Where(i => instrumentsIds.Contains(i.Id)).ToListAsync();
            
            // -- update instrument quantity in db -- //
            // -- spread instruments count -- //

            float totalPrice = 0;
            List<Instrument> orderInstruments = new();
            foreach(Instrument inst in dbInsts)
            {
                int left = inst.Quantity;
                if (left - ic_dict[inst.Id] < 0)  // -o-ut of stock
                    return JsonSuccess(false, new { msg = "o", left = left, inst = inst});
                inst.Quantity -= ic_dict[inst.Id];
                inst.Sold += ic_dict[inst.Id];
                
                _context.Update(inst);

                for (int j = 0; j < ic_dict[inst.Id]; j++)
                {
                    orderInstruments.Add(inst);
                    totalPrice += inst.Price;
                }    

            } await _context.SaveChangesAsync();

                /* CHARGE HERE !!!
                               if !charge.success =>
                                    reverse instruments quantity on db
                                    redirect to malfunction */

            Order order = new();
            try
            {
                order.UserId = uid;
                order.Address = dbUser.Address;

                order.Instruments = orderInstruments;
                order.TotalPrice= totalPrice;

                order.Create = DateTime.Now;
                order.Shipping = DateTime.Now.AddMonths(3);

                dbUser.InstrumentsWishlist = "";
                _context.Add(order);
                _context.Update(dbUser);
                await _context.SaveChangesAsync();
            } // alert -e-xception :
            catch(DbUpdateConcurrencyException) // -u-pdate exception
            { return JsonSuccess(false, new { msg = "u" }); } 
            

            return JsonSuccess(true, new { msg = "s" });

        }

        private bool OrderExists(int id)
        {
            return _context.Order.Any(e => e.Id == id);
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


        // @@ @@@@@@@@@@@@@@@ Reditection functions @@@@@@@@@@@@@@@@ @@ //

        [AllowAnonymous]
        private IActionResult RedirectToMalfunction()
        {
            return RedirectToAction("Malfunction", "Home");
        }


        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }        

        [AllowAnonymous]
        private IActionResult RedirectToAccessDenied()
        {
            return RedirectToAction("AccessDenied", "Users");
        }
    }
}

