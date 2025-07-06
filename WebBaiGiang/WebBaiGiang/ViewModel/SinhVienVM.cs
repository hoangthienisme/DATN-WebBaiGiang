using System.ComponentModel.DataAnnotations;

namespace WebBaiGiang.ViewModel
{
    public class SinhVienVM
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ tên sinh viên")]
        [StringLength(100, ErrorMessage = "Tên không được vượt quá 100 ký tự")]
        [Display(Name = "Họ và tên")]
        public string Name { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Địa chỉ email không hợp lệ")]
        [Display(Name = "Email")]
        public string Email { get; set; } = "";

        [MinLength(10, ErrorMessage = "Số điện thoại phải có ít nhất 10 chữ số")]
        [Display(Name = "Số điện thoại")]
        public string? Phone { get; set; }

        [Display(Name = "Đang hoạt động")]
        public bool IsActive { get; set; } = true;
    }
}
