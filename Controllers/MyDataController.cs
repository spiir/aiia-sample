using Microsoft.AspNetCore.Mvc;

namespace MyDataSample.Controllers
{
    public class MyDataController : Controller
    {
        // GET
        public IActionResult Index()
        {
            return
            View();
        }
    }
}