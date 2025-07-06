using System.ComponentModel.DataAnnotations;
using WebBaiGiang.Models;

namespace WebBaiGiang.ViewModel
{
    public class ChamBaiTapViewModel
    {
        [Required(ErrorMessage = "Thông tin bài tập không được để trống")]
        public BaiTap BaiTap { get; set; } = null!;

        [Required(ErrorMessage = "Danh sách nộp bài không được để trống")]
        public List<NopBai> DanhSachNop { get; set; } = new List<NopBai>();
    }
}
