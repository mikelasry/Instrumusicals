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

            List<Store> stores =  _context.Store.ToList();
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

                geolocations.Add(new List<double> { Convert.ToDouble(lng.Value), Convert.ToDouble(lat.Value)});

            }
            ));

            ViewData["Geolocations"] = JsonSerializer.Serialize(geolocations);//Json(geolocations.ToArray()).Value.ToString();
            ViewData["Stores"] = JsonSerializer.Serialize(stores.ToArray().Select(st => st.Name).ToArray());

            
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
