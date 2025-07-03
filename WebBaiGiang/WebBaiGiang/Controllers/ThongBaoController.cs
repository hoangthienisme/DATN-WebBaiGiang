using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebBaiGiang.Models;

namespace WebBaiGiang.Controllers
{
    public class ThongBaoController : Controller
    {
        private readonly WebBaiGiangContext _context;

        public ThongBaoController(WebBaiGiangContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetThongBao()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out int userId)) return Unauthorized();

            var tb = await _context.ThongBaos
                .Where(t => t.NguoiNhanId == userId)
                .OrderByDescending(t => t.ThoiGian)
                .Take(10)
                .ToListAsync();

            var result = tb.Select(t => new
            {
                t.Id,
                t.NoiDung,
                t.ThoiGian,
                t.DaDoc,
                t.LienKet
            });

            return Json(result);
        }

        [HttpPost]
        public async Task<IActionResult> DanhDauDaDoc(int id)
        {
            var tb = await _context.ThongBaos.FindAsync(id);
            if (tb != null)
            {
                tb.DaDoc = true;
                await _context.SaveChangesAsync();
            }

            return Ok();
        }
        [HttpPost]
        public async Task<IActionResult> DanhDauDaDocTheoLink(string link)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out int userId))
                return Unauthorized();

            var thongBao = await _context.ThongBaos
                .Where(tb => tb.NguoiNhanId == userId && tb.LienKet == link && !tb.DaDoc)
                .FirstOrDefaultAsync();

            if (thongBao != null)
            {
                thongBao.DaDoc = true;
                await _context.SaveChangesAsync();
            }

            return Ok();
        }

    }
}
