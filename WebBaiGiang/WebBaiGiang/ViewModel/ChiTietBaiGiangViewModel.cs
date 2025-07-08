using System.ComponentModel.DataAnnotations;
using WebBaiGiang.Models;

namespace WebBaiGiang.ViewModel
{
    public class ChiTietBaiGiangViewModel
    {
        public int Id { get; set; }

        public string? Title { get; set; }

        public string? Description { get; set; }

        public DateTime CreatedDate { get; set; }

        public List<TaiNguyen> TaiNguyens { get; set; } = new();  

        public List<Chuong> Chuongs { get; set; } = new();
        public int? HocPhanId { get; set; }
        public IFormFile[]? ImageFiles { get; set; }
        public IFormFile[]? DocumentFiles { get; set; }
        public string? YoutubeLinks { get; set; } // ngăn cách dấu phẩy

        public List<BinhLuan> BinhLuans { get; set; } = new();
        public List<HocPhan> HocPhans { get; set; } = new();
        public BaiGiang BaiGiang { get; set; } = new();

        // Có thể thêm: Form gửi bình luận
        [StringLength(1000, ErrorMessage = "Bình luận không được vượt quá 1000 ký tự")]
        public string NoiDungBinhLuanMoi { get; set; } = string.Empty;
    }
}
