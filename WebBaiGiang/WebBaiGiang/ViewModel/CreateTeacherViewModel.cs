using System.ComponentModel.DataAnnotations;

namespace WebBaiGiang.ViewModel
{
    public class CreateTeacherViewModel
    {
        [Required(ErrorMessage = "Tên không được để trống")]
        [StringLength(100)]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; }

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string? Phone { get; set; }

        // Password không bắt buộc, admin có thể để trống
        [DataType(DataType.Password)]
        public string? Password { get; set; }
    }
}
