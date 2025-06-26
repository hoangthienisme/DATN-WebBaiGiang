using Microsoft.AspNetCore.Mvc;
using WebBaiGiang.Models;

namespace WebBaiGiang.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ThongTinWebController : Controller
    {
        private readonly WebBaiGiangContext _context;
        private readonly ILogger<ThongTinWebController> _logger;

        public ThongTinWebController(WebBaiGiangContext context, ILogger<ThongTinWebController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Hiển thị form cập nhật (và tạo bản ghi nếu chưa có)
        public IActionResult Edit()
        {
            var thongTin = _context.ThongTinWebs.FirstOrDefault();

            if (thongTin == null)
            {
                thongTin = new ThongTinWeb
                {
                    Name = "Tên website",
                    TenTruong = "Tên trường",
                    CreatedDate = DateTime.Now,
                    FacebookLink = "",
                    YoutubeLink = "",
                    InstagramLink = ""
                };

                _context.ThongTinWebs.Add(thongTin);
                _context.SaveChanges();
            }

            return View(thongTin);
        }

        // Xử lý cập nhật thông tin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(ThongTinWeb model)
        {
            if (ModelState.IsValid)
            {
                var existing = _context.ThongTinWebs.FirstOrDefault(x => x.Id == model.Id);
                if (existing != null)
                {
                    // Cập nhật từng trường
                    existing.Name = model.Name;
                    existing.Description = model.Description;
                    existing.TenTruong = model.TenTruong;
                    existing.DiaChi = model.DiaChi;
                    existing.EmailLienHe = model.EmailLienHe;
                    existing.PhoneLienHe = model.PhoneLienHe;
                    existing.FacebookLink = model.FacebookLink;
                    existing.YoutubeLink = model.YoutubeLink;
                    existing.InstagramLink = model.InstagramLink;
                    existing.UpdateDate = DateTime.Now;
                    // Xử lý upload logo
                    if (model.LogoFile != null && model.LogoFile.Length > 0)
                    {
                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.LogoFile.FileName);
                        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/logo", fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            model.LogoFile.CopyTo(stream);
                        }

                        // Gán đường dẫn logo
                        existing.LogoUrl = "/logo/" + fileName;
                    }
                    _context.SaveChanges();
                    TempData["Message"] = "Cập nhật thông tin thành công!";
                }
                else
                {
                    TempData["Message"] = "Không tìm thấy bản ghi để cập nhật.";
                }

                return RedirectToAction("Index","Home", new { area = "Admin" });
            }

            return View(model);
        }
    }
}
