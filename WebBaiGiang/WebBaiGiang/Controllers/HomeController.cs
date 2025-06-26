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

    public async Task<IActionResult> Index(int id)
    {
        var thongTin = _context.ThongTinWebs
            .OrderByDescending(t => t.UpdateDate ?? t.CreatedDate) 
            .FirstOrDefault();

        ViewBag.FooterData = thongTin;
        return View();
    }


    public IActionResult About()
    {
        return View();
    } 

    //[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    //public IActionResult Error()
    //{
    //    return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    //}
}