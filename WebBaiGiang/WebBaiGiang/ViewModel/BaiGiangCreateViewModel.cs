using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using WebBaiGiang.Models;

namespace WebBaiGiang.ViewModel
{
    public class BaiGiangCreateViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tên bài giảng!")]
        [StringLength(200, ErrorMessage = "Tên bài giảng không vượt quá 200 ký tự")]
        [RegularExpression(@"^(?=.*[\p{L}])[\p{L}\p{M} \.'\-]+$", ErrorMessage = "Tên bài giảng không được chứa ký tự đặc biệt hoặc để trống")]
        public string Title { get; set; } = null!;
        public string? Description { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ít nhất một lớp học!")]
        public List<int> SelectedClassIds { get; set; } = new();

        [Required(ErrorMessage = "Vui lòng chọn học phần!")]
        public int HocPhanId { get; set; }

        public List<SelectListItem> AvailableHocPhans { get; set; } = new();
        public List<SelectListItem> AvailableClasses { get; set; } = new();
        public List<TaiNguyen>? TempTaiNguyenImages { get; set; } = new();
        public List<TaiNguyen>? TempTaiNguyenDocs { get; set; } = new();
        public List<TaiNguyen>? TempYoutubeLinks { get; set; } = new();

        public List<IFormFile>? ImageFiles { get; set; }
        public List<IFormFile>? DocumentFiles { get; set; }

        [MaxLength(10, ErrorMessage = "Chỉ nhập tối đa 10 liên kết YouTube")]
        [YoutubeLinksValidation(ErrorMessage = "Một hoặc nhiều URL YouTube không hợp lệ")]
        public List<string>? YoutubeLinks { get; set; } = new();

        public List<ChuongCreateViewModel> Chuongs { get; set; } = new();
    }

    public class ChuongCreateViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tên chương!")]
        [StringLength(200, ErrorMessage = "Tên chương không vượt quá 200 ký tự")]
        public string Title { get; set; } = null!;

        public string? Description { get; set; }

        [Range(0, 1000, ErrorMessage = "Thứ tự phải là số không âm")]
        public int SortOrder { get; set; }

        public List<BaiCreateViewModel> Bais { get; set; } = new();
    }

    public class BaiCreateViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tên bài học!")]
        [StringLength(200, ErrorMessage = "Tên bài học không vượt quá 200 ký tự")]
        public string Title { get; set; } = null!;
        public string? Description { get; set; }

        [Url(ErrorMessage = "URL video không hợp lệ!")]
        [StringLength(500, ErrorMessage = "URL video quá dài")]
        public string? VideoUrl { get; set; }

        [Range(0, 1000, ErrorMessage = "Thứ tự phải là số không âm")]
        public int SortOrder { get; set; }

        public List<IFormFile>? ImageFiles { get; set; }
        public List<IFormFile>? DocumentFiles { get; set; }
        public List<string>? YouTubeLinks { get; set; }
    }

    // ✅ Custom validation attribute cho danh sách link YouTube
    public class YoutubeLinksValidationAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value is not List<string> links) return true;
            foreach (var link in links)
            {
                if (!string.IsNullOrWhiteSpace(link) &&
                    !Uri.IsWellFormedUriString(link, UriKind.Absolute))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
