using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WebBaiGiang.Models;
using Microsoft.EntityFrameworkCore;

namespace WebBaiGiang.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly WebBaiGiangContext _context;

    public HomeController(ILogger<HomeController> logger, WebBaiGiangContext context)
    {
        _logger = logger;
        _context = context;
    }

    public IActionResult Index()
    {

        var viewModel = new HomeViewModel
        {
            // 1. Khóa học nổi bật (lấy 5 khóa học từ Course)
            FeaturedCourses = _context.Courses
                     .OrderBy(c => c.IdCourse)
                     .Take(5)
                     .ToList(),

            // 2. Khóa học của giảng viên (lấy từ ClassCourse)
            Courses = _context.ClassCourses
                     .Include(c => c.IdCourseNavigation)
                     .Include(c => c.IdLecturerNavigation)
                         .ThenInclude(l => l.IdUserNavigation)
                     .Where(c => c.Semester == "HK1-2025") // Học kỳ hiện tại
                     .ToList(),

            // 3. Đề xuất cho bạn (lấy 4 khóa học ngẫu nhiên từ Course)
            Recommendations = _context.Courses
                     .OrderBy(r => Guid.NewGuid()) // Random
                     .Take(4)
                     .ToList(),

            // 4. Phản hồi từ người dùng (lấy từ Feedback)
            Feedbacks = _context.Feedbacks
                     .Include(f => f.IdStudentNavigation)
                         .ThenInclude(s => s.IdUserNavigation)
                     .OrderByDescending(f => f.IdFeedback)
                     .Take(5)
                     .ToList()
        };
        return View(viewModel);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}