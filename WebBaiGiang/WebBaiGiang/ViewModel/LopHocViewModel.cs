using System.ComponentModel.DataAnnotations;
using WebBaiGiang.Models;

namespace WebBaiGiang.ViewModel
{
    public class LopHocViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên lớp học")]
        [RegularExpression(@"^[\w\s\-\(\)\[\]À-ỹà-ỹÁ-Ýá-ý]+$", ErrorMessage = "Tên lớp học chứa ký tự không hợp lệ")]
        [Display(Name = "Tên lớp học")]
        public string Name { get; set; } = null!;

        [Display(Name = "Ảnh đại diện lớp (tuỳ chọn)")]
        public string? Picture { get; set; }

        // Danh sách bài giảng chưa gán cho lớp học
        public List<BaiGiang>? BaiGiangsChuaCo { get; set; }

        [Required(ErrorMessage = "Không thể hiển thị bài giảng (trang rỗng)")]
        public PhanTrang<BaiGiang> BaiGiangs { get; set; } = default!;

        [Required(ErrorMessage = "Không thể hiển thị bài tập (trang rỗng)")]
        public PhanTrang<BaiTap> BaiTaps { get; set; } = default!;

        // Danh sách sinh viên trong lớp
        public List<NguoiDung>? Students { get; set; }
        // Danh sách giảng viên trong lớp
        public List<NguoiDung>? Teachers { get; set; }

    }
}
