using Instrumusicals.Data;
using Instrumusicals.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
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
            // Retreive best sellers from remote DB
            List<Instrument> bestSellers = _context.Instrument
                .OrderByDescending(i => i.Sold).Take(3).ToList();
            ViewData["BestSellers"] = bestSellers;
            // Retreive most reviewed instruments from remote DB
            List<Instrument> mostReviewed = _context.Instrument
                .OrderByDescending(i => i.Reviews.Count()).Take(3).ToList();
            ViewData["MostReviewed"] = mostReviewed;
            // Retreive Top 10 Albums of all times using RapidAPI documentation and TheAudioDB open source project.
            var client = new HttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri("https://theaudiodb.p.rapidapi.com/mostloved.php?format=album"),
                Headers =
                    {
                        { "x-rapidapi-host", "theaudiodb.p.rapidapi.com" },
                        { "x-rapidapi-key", "2a837bc24fmsh5f0df059b95a537p14ec1djsn6e5e40a62e16" },
                    },
            };
            var response =  client.Send(request);
            response.EnsureSuccessStatusCode();
            var result = response.Content.ReadAsStringAsync().Result;
            List<TopTenItem> topTen = new();
            JObject json = (JObject)JsonConvert.DeserializeObject(result);
            var dictionary = new InsensitiveWrapper(json);
            // Sort topTen
            for(var i = 0; i < 20; i++)
            {
                var AlbumName_ = dictionary._rWrapped["loved"][i]["strAlbum"].ToString();
                var ArtistName_ = dictionary._rWrapped["loved"][i]["strArtist"].ToString();
                var YearReleased_ = dictionary._rWrapped["loved"][i]["intYearReleased"].ToString();
                var Style_ = dictionary._rWrapped["loved"][i]["strStyle"].ToString();
                var Case3D_ = dictionary._rWrapped["loved"][i]["strAlbum3DCase"].ToString();
                var Sales_ = dictionary._rWrapped["loved"][i]["intSales"].ToString();

                TopTenItem item = new TopTenItem(AlbumName_, ArtistName_, YearReleased_, Style_, Case3D_, Sales_);
                topTen.Add(item);
            }
            topTen.Sort((TopTenItem it1, TopTenItem it2) => { return Int32.Parse(it2.Sales) - Int32.Parse(it1.Sales); });
            ViewData["TopAlbums"] = topTen.GetRange(0,5);

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




        public class InsensitiveWrapper
        {
            public readonly JObject _rWrapped;
            public readonly string _rLeafValue;

            public InsensitiveWrapper(JObject jsonObj)
            {
                _rWrapped = jsonObj ?? throw new ArgumentNullException(nameof(jsonObj));
            }

            private InsensitiveWrapper(string value)
            {
                _rLeafValue = value ?? throw new ArgumentNullException(nameof(value));
            }

            public string Value => _rLeafValue ?? throw new InvalidOperationException("Value can be retrieved only from leaf.");

            public InsensitiveWrapper this[string key]
            {
                get
                {
                    object nonTyped = _rWrapped.GetValue(key, StringComparison.OrdinalIgnoreCase);
                    if (nonTyped == null)
                        throw new KeyNotFoundException($"Key {key} is not found.");


                    JObject jObject = nonTyped as JObject;
                    if (jObject == null)
                        return new InsensitiveWrapper(nonTyped.ToString());

                    return new InsensitiveWrapper(jObject);
                }
            }
        }
    }

    
}
