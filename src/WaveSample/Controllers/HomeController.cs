using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace WaveSample.Controllers
{
    public class HomeController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(IFormFile File, int Count)
        {
            var memStream = new MemoryStream();
            await File.CopyToAsync(memStream);
            using (var wave = new WaveStream(memStream, 4, 42))
            {
                return View("Show", await wave.GetSampleAsync(Count));
            }
        }
    }
}
