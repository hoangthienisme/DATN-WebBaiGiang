using Microsoft.AspNetCore.Mvc;

namespace WebBaiGiang.Controllers
{
    public class StudentController : Controller
    {
        public IActionResult Dashboard()
        {
            return View();
        }
    }
}
