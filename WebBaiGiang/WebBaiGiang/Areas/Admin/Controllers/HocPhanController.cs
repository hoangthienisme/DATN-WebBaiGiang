using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebBaiGiang.Models; 
using Microsoft.AspNetCore.Authorization; 
using System.Security.Claims;

namespace WebBaiGiang.Areas.Admin.Controllers
{
    [Area("Admin")] 
    public class HocPhanController : Controller
    {
        private readonly WebBaiGiangContext _context;

        public HocPhanController(WebBaiGiangContext context)
        {
            _context = context;
        }

     
        public async Task<IActionResult> HocPhan(string search, int page = 1, int pageSize = 10)
        {
            if (pageSize <= 0) pageSize = 10;
            if (page <= 0) page = 1;
            var hocPhans = _context.HocPhans.Include(h => h.Department).AsQueryable();
            if (!string.IsNullOrEmpty(search))
            {
                hocPhans = hocPhans.Where(h => h.Name.Contains(search) ||
                                                 (h.Description != null && h.Description.Contains(search)));
                ViewBag.CurrentSearch = search; 
            }
            var totalItems = await hocPhans.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            // Áp dụng phân trang: bỏ qua các mục ở các trang trước và lấy số lượng mục của trang hiện tại
            var itemsToShow = await hocPhans
                                    .OrderBy(h => h.Id) // Sắp xếp để phân trang nhất quán
                                    .Skip((page - 1) * pageSize)
                                    .Take(pageSize)
                                    .ToListAsync();

            // Truyền dữ liệu phân trang và các biến cần thiết tới View
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.PageSize = pageSize; // Để tùy chỉnh kích thước trang nếu cần
            ViewBag.TotalItems = totalItems;

            return View(itemsToShow);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var hocPhan = await _context.HocPhans
                .Include(h => h.Department)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (hocPhan == null)
            {
                return NotFound();
            }

            return View(hocPhan);
        }
        [HttpGet]
        public IActionResult Create()
        {
            var departments = _context.Khoas.Where(k => k.IsActive).ToList();
            ViewBag.DepartmentId = new SelectList(departments, "Id", "Name");
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create( HocPhan hocPhan)
        { 
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdStr, out int userId))
            {

                ModelState.AddModelError("", "Không xác định được người tạo.");
                return View(hocPhan);
            }
            if (ModelState.IsValid)
            {
                hocPhan.CreatedDate = DateTime.Now;
                hocPhan.CreatedBy = userId;
                _context.Add(hocPhan);
                await _context.SaveChangesAsync();
                return RedirectToAction("HocPhan", "HocPhan", new { area = "Admin" });
            }
            // Nếu dữ liệu không hợp lệ, tải lại form với các lỗi
            var departments = _context.Khoas.Where(k => k.IsActive).ToList();
            ViewData["DepartmentId"] = new SelectList(_context.Khoas, "Id", "Name", hocPhan.DepartmentId);
            return RedirectToAction("HocPhan", "HocPhan", new { area = "Admin" });

        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var hocPhan = await _context.HocPhans.FindAsync(id);
            if (hocPhan == null)
            {
                return NotFound();
            }
            var departments = _context.Khoas.Where(k => k.IsActive).ToList();
            ViewBag.DepartmentId = new SelectList(_context.Khoas, "Id", "Name", hocPhan.DepartmentId);
            return View(hocPhan);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, HocPhan hocPhan)
        {
            if (id != hocPhan.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var hocPhanOld = await _context.HocPhans.FindAsync(id);
                    if (hocPhanOld == null)
                    {
                        return NotFound();
                    }

                    hocPhanOld.Name = hocPhan.Name;
                    hocPhanOld.Description = hocPhan.Description;
                    hocPhanOld.DepartmentId = hocPhan.DepartmentId;
                    hocPhanOld.UpdateDate = DateTime.Now;

                    var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (int.TryParse(userIdStr, out int userId))
                    {
                        hocPhanOld.UpdateBy = userId;
                    }

                    await _context.SaveChangesAsync();
                    return RedirectToAction("HocPhan", "HocPhan", new { area = "Admin" });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!HocPhanExists(hocPhan.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            ViewBag.DepartmentId = new SelectList(_context.Khoas.Where(k => k.IsActive), "Id", "Name", hocPhan.DepartmentId);
            return RedirectToAction("HocPhan", "HocPhan", new { area = "Admin" });

        }


        public async Task<IActionResult> An(int id)
        {
            var hocPhan = await _context.HocPhans.FindAsync(id);
            if (hocPhan != null)
            {
                hocPhan.IsActive = false;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("HocPhan", "HocPhan", new { area = "Admin" });

        }

        public async Task<IActionResult> KhoiPhuc(int id)
        {
            var hocPhan = await _context.HocPhans.FindAsync(id);
            if (hocPhan != null)
            {
                hocPhan.IsActive = true;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("HocPhan", "HocPhan", new { area = "Admin" });

        }



        private bool HocPhanExists(int id)
        {
            return _context.HocPhans.Any(e => e.Id == id);
        }
    }
}
