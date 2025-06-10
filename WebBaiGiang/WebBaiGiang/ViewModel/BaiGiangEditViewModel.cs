using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace WebBaiGiang.ViewModel
{
    public class BaiGiangEditViewModel
    {
        public int Id { get; set; } // Thêm Id cho trường hợp Edit

        [Required(ErrorMessage = "Tiêu đề bài giảng không được để trống.")]
        public string Title { get; set; }

        public string? Description { get; set; }

        public IFormFile? Attachment { get; set; }
        public string? ExistingAttachmentUrl { get; set; } // Để hiển thị file đã có

        // Danh sách các lớp học khả dụng cho dropdown
        [Display(Name = "Chọn lớp áp dụng")]
        public List<SelectListItem>? AvailableClasses { get; set; }

        // Danh sách ID của các lớp đã chọn (được submit từ form)
        [Required(ErrorMessage = "Vui lòng chọn ít nhất một lớp học.")]
        public List<int> SelectedClassIds { get; set; } = new List<int>();

        // Danh sách các chương
        public List<ChuongViewModel> Chuongs { get; set; } = new List<ChuongViewModel>();
    }
    public class ChuongViewModel
    {
        public int Id { get; set; } // Cho trường hợp Edit chương đã có
        [Required(ErrorMessage = "Tiêu đề chương không được để trống.")]
        public string Title { get; set; }
        public string? Description { get; set; }
        public int SortOrder { get; set; } // Thứ tự sắp xếp

        public List<BaiViewModel> Bais { get; set; } = new List<BaiViewModel>();
    }

    public class BaiViewModel
    {
        public int Id { get; set; } // Cho trường hợp Edit bài đã có
        [Required(ErrorMessage = "Tiêu đề bài học không được để trống.")]
        public string Title { get; set; }
        public string? Description { get; set; }
        public string? VideoUrl { get; set; }
        public int SortOrder { get; set; } // Thứ tự sắp xếp

        public IFormFile? DocumentFile { get; set; }
        public string? ExistingDocumentUrl { get; set; } // Để hiển thị file đã có
    }
}
