﻿using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace WebBaiGiang.ViewModel
{
    public class BaiGiangCreateViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tên bài giảng!")]
        public string Title { get; set; } = null!;

        public string? Description { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ít nhất một lớp học!")]
        public List<int> SelectedClassIds { get; set; } = new List<int>();

        public List<IFormFile>? ImageFiles { get; set; }
        public List<IFormFile>? DocumentFiles { get; set; }
        public List<SelectListItem> AvailableClasses { get; set; } = new List<SelectListItem>();

        public List<ChuongCreateViewModel> Chuongs { get; set; } = new List<ChuongCreateViewModel>();
    }

    public class ChuongCreateViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tên chương!")]
        public string Title { get; set; } = null!;

        public string? Description { get; set; }

        public int SortOrder { get; set; }

        public List<BaiCreateViewModel> Bais { get; set; } = new List<BaiCreateViewModel>();
    }

    public class BaiCreateViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tên bài học!")]
        public string Title { get; set; } = null!;

        public string? Description { get; set; }

        [Url(ErrorMessage = "URL video không hợp lệ!")]
        public string? VideoUrl { get; set; }

        public IFormFile? DocumentFile { get; set; } // Đổi từ Attachment thành DocumentFile để khớp với view

        public int SortOrder { get; set; }
    }

    // ViewModel for editing existing chapters
    public class ChuongEditViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên chương!")]
        public string Title { get; set; } = null!;

        public string? Description { get; set; }

        public int SortOrder { get; set; }

        public List<BaiEditViewModel> Bais { get; set; } = new List<BaiEditViewModel>();
    }

    // ViewModel for editing existing lessons (Bai)
    public class BaiEditViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên bài học!")]
        public string Title { get; set; } = null!;

        public string? Description { get; set; }

        [Url(ErrorMessage = "URL video không hợp lệ!")]
        public string? VideoUrl { get; set; }

        public IFormFile? DocumentFile { get; set; } // For uploading new file

        public string? Document { get; set; } // To store the existing file URL

        public int SortOrder { get; set; }
    }
}