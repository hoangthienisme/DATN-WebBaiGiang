using System.ComponentModel.DataAnnotations;
using WebBaiGiang.Models;

namespace WebBaiGiang.ViewModel
{
    public class InviteUsersViewModel
    {
        public int ClassId { get; set; }

        // Dùng để submit email mời
        [Required(ErrorMessage = "Vui lòng nhập Email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; }

        // Xác định loại người dùng: "Student" hoặc "Teacher"
        public string Role { get; set; }

        // Danh sách hiển thị
        public List<NguoiDung> Students { get; set; } = new();
        public List<NguoiDung> Teachers { get; set; } = new();
    }
}
