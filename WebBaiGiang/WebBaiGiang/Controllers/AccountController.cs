using Microsoft.AspNetCore.Mvc;

namespace WebBaiGiang.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult SignUp()
        {
            return View();
        }
        public IActionResult Login()
        {
            return View();
        }
    }
}
