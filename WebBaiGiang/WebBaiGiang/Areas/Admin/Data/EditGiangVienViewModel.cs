using System.ComponentModel.DataAnnotations;

namespace WebBaiGiang.Areas.Admin.Data
{
    public class EditGiangVienViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên bắt buộc")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email bắt buộc")]
        public string Email { get; set; }

        public string? Phone { get; set; }
    }
}
