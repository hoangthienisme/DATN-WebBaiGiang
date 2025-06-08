using Microsoft.AspNetCore.Mvc;
using WebBaiGiang.Models;

namespace WebBaiGiang.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class KhoaController : Controller
    {
        private static List<Khoa> dsKhoa = new List<Khoa>
        {
            new Khoa { Id = 1, Name = "Công nghệ thông tin", Description = "Khoa CNTT" },
            new Khoa { Id = 2, Name = "Kinh tế", Description = "Khoa Kinh tế" },
            new Khoa { Id = 3, Name = "Xây dựng", Description = "Khoa Xây dựng" }
        };

        public IActionResult Khoa(string search)
        {
            IEnumerable<Khoa> model = dsKhoa;

            if (!string.IsNullOrEmpty(search))
            {
                model = model.Where(k => k.Name.Contains(search, System.StringComparison.OrdinalIgnoreCase));
            }

            return View(model);
        }
    }
}
