using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace example.WebApp.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View();
        }

        [Authorize(Policy = "AdminPolicy")]
        public IActionResult Administration()
        {
            ViewData["Message"] = "Administrators only.";

            return View();
        }

        [Authorize(Policy = "MembersOnlyPolicy")]
        public IActionResult Members()
        {
            ViewData["Message"] = "Members only.";

            return View();
        }
    }
}
