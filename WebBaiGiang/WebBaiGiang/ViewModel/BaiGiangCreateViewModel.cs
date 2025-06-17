using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace WebBaiGiang.ViewModel
{
    public class BaiGiangCreateViewModel 
    {
        [Required(ErrorMessage = "Vui lòng nhập tên bài giảng!")]
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public List<int> SelectedClassIds { get; set; } = new List<int>();
        public List<IFormFile>? Attachments { get; set; }
        public List<IFormFile>? DocumentFiles { get; set; }   
        public List<SelectListItem> AvailableClasses { get; set; } = new List<SelectListItem>();
        public List<ChuongCreateViewModel> Chuongs { get; set; } = new List<ChuongCreateViewModel>();
    }

    public class ChuongCreateViewModel
    {
        public string Title { get; set; } = null!;
        public string? Description { get; set; } // Added Description for Chuong
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

    
    // New ViewModel for editing existing chapters
    public class ChuongEditViewModel
    {
        public int Id { get; set; } // Chapter ID for existing chapters
        [Required(ErrorMessage = "Vui lòng nhập tên chương!")]
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public int SortOrder { get; set; }
        public List<BaiEditViewModel> Bais { get; set; } = new List<BaiEditViewModel>(); // Changed to BaiEditViewModel
    }

    // New ViewModel for editing existing lessons (Bai)
    public class BaiEditViewModel
    {
        public int Id { get; set; } // Lesson ID for existing lessons
        [Required(ErrorMessage = "Vui lòng nhập tên bài học!")]
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public string? VideoUrl { get; set; }
        public IFormFile? DocumentFile { get; set; } // For uploading new file
        public string? Document { get; set; } // To store the existing file URL
        public int SortOrder { get; set; }
    }
}