using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace WebBaiGiang.ViewModel
{
    public class TaoBaiTapViewModel
    {
        [Required(ErrorMessage = "Tiêu đề bài tập là bắt buộc")]
        public string Title { get; set; }

        public string? Description { get; set; }

        [Required(ErrorMessage = "Phải chọn ít nhất 1 lớp")]
        public List<int> ClassIds { get; set; } = new List<int>();
        public int? MaxScore { get; set; }

        public IFormFile? Attachment { get; set; } // File đính kèm

        [DataType(DataType.Date)]
        public DateTime? DueDate { get; set; }
        public int LopIdGoc { get; set; }
        // Nếu muốn hiển thị danh sách lớp trong view
        [ValidateNever]
        public List<SelectListItem> AvailableClasses { get; set; }

       
       
    }
}
