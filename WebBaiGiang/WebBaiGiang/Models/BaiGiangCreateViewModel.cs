using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace WebBaiGiang.Models
{
    public class BaiGiangCreateViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tên bài giảng!")]
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public List<int> ClassIds { get; set; } = new List<int>();
        public List<SelectListItem> AvailableClasses { get; set; } = new List<SelectListItem>();
        public IFormFile? Attachment { get; set; }
        public List<ChuongCreateViewModel> Chuongs { get; set; } = new List<ChuongCreateViewModel>();
    }
    public class ChuongCreateViewModel
    {
        public string Title { get; set; } = null!;

        public int SortOrder { get; set; }

        public List<BaiCreateViewModel> Bais { get; set; } = new List<BaiCreateViewModel>();
    }

    public class BaiCreateViewModel
    {
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public string? VideoUrl { get; set; }

        public IFormFile? DocumentFile { get; set; }

        public int SortOrder { get; set; }
    }
}
