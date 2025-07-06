using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace WebBaiGiang.ViewModel
{
    public class TaoBaiTapViewModel
    {
        [Required(ErrorMessage = "Tiêu đề bài tập là bắt buộc")]
        [Display(Name = "Tiêu đề")]
        public string Title { get; set; } = null!;

        [Display(Name = "Mô tả")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Phải chọn ít nhất một lớp học")]
        [Display(Name = "Lớp áp dụng")]
        public List<int> ClassIds { get; set; } = new List<int>();

        [Range(0, 100, ErrorMessage = "Điểm tối đa phải nằm trong khoảng 0 - 100")]
        [Display(Name = "Điểm tối đa")]
        public int? MaxScore { get; set; }

        [Display(Name = "Tệp đính kèm")]
        public IFormFile? Attachment { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Hạn nộp")]
        public DateTime? DueDate { get; set; }

        [Display(Name = "Lớp gốc")]
        public int LopIdGoc { get; set; }

        [ValidateNever]
        [Display(Name = "Danh sách lớp")]
        public List<SelectListItem> AvailableClasses { get; set; } = new();
    }
}
