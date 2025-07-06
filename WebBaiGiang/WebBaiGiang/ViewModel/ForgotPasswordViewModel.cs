using System.ComponentModel.DataAnnotations;

namespace WebBaiGiang.ViewModel
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Định dạng email không hợp lệ")]
        [Display(Name = "Địa chỉ Email")]
        [RegularExpression(@"^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$", ErrorMessage = "Định dạng email không đúng")]
        public string Email { get; set; } = null!;
    }
}
