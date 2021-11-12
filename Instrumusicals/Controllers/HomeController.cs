using Instrumusicals.Data;
using Instrumusicals.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Instrumusicals.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly InstrumusicalsContext _context;

        public HomeController(ILogger<HomeController> logger, InstrumusicalsContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index() {
            List<Instrument> bestSellers = _context.Instrument
                .OrderByDescending(i => i.Sold).Take(3).ToList();
            ViewData["BestSellers"] = bestSellers;

            List<Instrument> mostReviewed = _context.Instrument
                .OrderByDescending(i => i.Reviews.Count()).Take(3).ToList();
            ViewData["MostReviewed"] = mostReviewed;
            
            return View();
        }

        public IActionResult Malfunction()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
