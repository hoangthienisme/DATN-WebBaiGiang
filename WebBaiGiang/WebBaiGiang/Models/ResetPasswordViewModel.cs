using System.ComponentModel.DataAnnotations;

namespace WebBaiGiang.Models
{
    public class ResetPasswordViewModel
    {
        
            [Required]
            public string Token { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string NewPassword { get; set; }

            [Required]
            [DataType(DataType.Password)]
            [Compare("NewPassword", ErrorMessage = "Mật khẩu xác nhận không khớp.")]
            public string ConfirmPassword { get; set; }
        }

    
}
