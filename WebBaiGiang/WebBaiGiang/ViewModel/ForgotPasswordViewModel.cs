using System.ComponentModel.DataAnnotations;

namespace WebBaiGiang.ViewModel
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
