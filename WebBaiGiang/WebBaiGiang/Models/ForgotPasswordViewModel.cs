using System.ComponentModel.DataAnnotations;

namespace WebBaiGiang.Models
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
