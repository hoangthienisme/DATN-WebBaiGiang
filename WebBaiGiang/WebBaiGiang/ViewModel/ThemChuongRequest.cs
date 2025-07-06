using System.ComponentModel.DataAnnotations;

namespace WebBaiGiang.ViewModel
{
    public class ThemChuongRequest
    {
        [Required]
        public int BaiGiangId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tiêu đề chương!")]
        [StringLength(200, ErrorMessage = "Tiêu đề chương không được vượt quá 200 ký tự")]
        [Display(Name = "Tiêu đề chương")]
        public string Title { get; set; } = null!;
    }
}
