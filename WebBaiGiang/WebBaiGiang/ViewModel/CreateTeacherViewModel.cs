using System.ComponentModel.DataAnnotations;

namespace WebBaiGiang.ViewModel
{
    public class CreateTeacherViewModel
    {
        [Required(ErrorMessage = "Tên không được để trống")]
        [StringLength(100, ErrorMessage = "Tên không được dài quá 100 ký tự")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [StringLength(150, ErrorMessage = "Email không được dài quá 150 ký tự")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [RegularExpression(@"^\d{10,}$", ErrorMessage = "Số điện thoại phải có ít nhất 10 chữ số")]
        public string? Phone { get; set; }
        // Password không bắt buộc, admin có thể để trống
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải từ 6 đến 100 ký tự")]
        public string? Password { get; set; }
    }
}
