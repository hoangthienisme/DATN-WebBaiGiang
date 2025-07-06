using System.ComponentModel.DataAnnotations;
using WebBaiGiang.Models;

namespace WebBaiGiang.ViewModel
{
    public class ChiTietBaiTapViewModel
    {
        [Required(ErrorMessage = "Dữ liệu bài tập không được để trống")]
        public BaiTap BaiTap { get; set; } = null!;

        // Có thể null nếu sinh viên chưa nộp bài
        public NopBai? NopBai { get; set; }
    }
}
