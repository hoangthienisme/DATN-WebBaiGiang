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

        public List<BinhLuan> BinhLuans { get; set; } = new();
        public BaiGiang BaiGiang { get; set; } = new();

        // Có thể thêm: Form gửi bình luận
        public string NoiDungBinhLuanMoi { get; set; } = string.Empty;
    }
}
