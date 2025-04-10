using Microsoft.AspNetCore.Mvc;

namespace WebBaiGiang.Controllers
{
    public class GiangVienController : Controller
    {
        public IActionResult Courses()
        {
            return View();
        }
        public IActionResult CreateCourses()
        {
            return View();
        }
    }
}
