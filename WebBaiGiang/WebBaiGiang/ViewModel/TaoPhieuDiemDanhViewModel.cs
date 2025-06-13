using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace WebBaiGiang.ViewModel
{
    public class TaoPhieuDiemDanhViewModel
    {
        [Required(ErrorMessage = "Vui lòng chọn ít nhất một lớp")]
        [Display(Name = "Các lớp điểm danh")]
        public List<int> ClassIds { get; set; } = new List<int>();

        public List<SelectListItem> AvailableClasses { get; set; } = new();
        [Display(Name = "Thời gian hết hạn (phút)")]
        [Range(1, 1440, ErrorMessage = "Thời gian hết hạn phải từ 1 đến 1440 phút")]
        public int? ExpiredInMinutes { get; set; }  // số phút hết hạn
        public int LopIdGoc { get; set; }
    }
}
