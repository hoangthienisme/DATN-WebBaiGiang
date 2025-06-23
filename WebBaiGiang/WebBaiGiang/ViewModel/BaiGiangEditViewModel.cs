using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace WebBaiGiang.ViewModel
{
    public class BaiGiangEditViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên bài giảng!")]
        public string Title { get; set; } = null!;

        public string? Description { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ít nhất một lớp học!")]
        public List<int> SelectedClassIds { get; set; } = new();

        public List<IFormFile>? ImageFiles { get; set; }             // Ảnh tổng
        public List<IFormFile>? DocumentFiles { get; set; }          // Tài liệu tổng
        public List<string>? YoutubeLinks { get; set; } = new();     // Link YouTube tổng

        // Dữ liệu cũ (để hiện ra và chọn xóa)
        public List<TaiNguyenViewModel> ExistingImages { get; set; } = new();     // Ảnh tổng cũ
        public List<TaiNguyenViewModel> ExistingDocuments { get; set; } = new();  // Tài liệu tổng cũ
        public List<string> ExistingYoutubeLinks { get; set; } = new();           // Link youtube cũ

        public List<SelectListItem> AvailableClasses { get; set; } = new();
        public List<ChuongEditViewModel> Chuongs { get; set; } = new();
    }



    public class ChuongEditViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên chương!")]
        public string Title { get; set; } = null!;

        public string? Description { get; set; }

        public int SortOrder { get; set; }

        public List<BaiEditViewModel> Bais { get; set; } = new();
    }

    public class BaiEditViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên bài học!")]
        public string Title { get; set; } = null!;

        public string? Description { get; set; }

        [Url(ErrorMessage = "URL video không hợp lệ!")]
        public string? VideoUrl { get; set; }

        public int SortOrder { get; set; }

        public List<IFormFile>? ImageFiles { get; set; }
        public List<IFormFile>? DocumentFiles { get; set; }

        public List<string>? ExistingImageUrls { get; set; } = new();
        public List<string>? ExistingDocumentUrls { get; set; } = new();
    }
}
