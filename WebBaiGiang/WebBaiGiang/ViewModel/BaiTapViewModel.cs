using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace WebBaiGiang.ViewModel
{
    public class BaiTapViewModel
    {
        [Required(ErrorMessage = "Tiêu đề bài tập là bắt buộc")]
        public string Title { get; set; } = null!;

        public string? Description { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Name = "Hạn nộp")]
        public DateTime? DueDate { get; set; }

        [Display(Name = "Link tài liệu")]
        public string? ContentUrl { get; set; }

        [Display(Name = "Điểm tối đa")]
        [Range(0, 100, ErrorMessage = "Điểm tối đa không được vượt quá 100.")]
        public double? MaxPoint { get; set; } = 100;


        [Display(Name = "Tệp đính kèm")]
        public IFormFile? Attachment { get; set; }

        // ID của các lớp được chọn
        [Display(Name = "Các lớp nhận bài tập")]
        public List<int> ClassIds { get; set; } = new List<int>();

        // Danh sách lớp hiển thị ở checkbox
        public List<SelectListItem>? AvailableClasses { get; set; }

        // ID lớp gốc dùng để quay lại sau khi submit (ẩn)
        public int LopIdGoc { get; set; }
    }
}
