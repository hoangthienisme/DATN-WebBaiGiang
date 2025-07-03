using System.Collections.Generic;
using WebBaiGiang.Models;

namespace WebBaiGiang.ViewModel
{
    public class BinhLuanViewModel
    {
        public List<BinhLuan>? BinhLuans { get; set; }
        public int BaiGiangId { get; set; }
        public int CurrentUserId { get; set; }
        public string? CurrentUserRole { get; set; }
    }
}
