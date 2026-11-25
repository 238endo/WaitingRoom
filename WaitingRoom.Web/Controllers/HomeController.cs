using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WaitingRoom.Web.Models;

namespace WaitingRoom.Web.Controllers
{
    public class HomeController : Controller
    {
        /// <summary>
        /// トップページを表示するビューを返します。
        /// </summary>
        public IActionResult Index()
        {
            return View();
        }

        /*
        public IActionResult Privacy()
        {
            return View();
        }
        */

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
