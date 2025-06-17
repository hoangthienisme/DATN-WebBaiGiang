namespace WebBaiGiang.Models
{
    public class TaiNguyen
    {
        public int Id { get; set; }

        public int BaiGiangId { get; set; }
        public BaiGiang BaiGiang { get; set; } = null!;

        public string Url { get; set; } = null!;
        public string Loai { get; set; } = null!; // "image" hoặc "video"
    }
}
