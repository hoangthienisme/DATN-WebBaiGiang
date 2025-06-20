namespace WebBaiGiang.Models
{
    public class TaiNguyen
    {
        public int Id { get; set; }

        public int? BaiGiangId { get; set; }
        public int? BaiId { get; set; }
        public BaiGiang? BaiGiang { get; set; } = null!;
        public Bai? Bai { get; set; } // Nullable because it can be null if this resource is not associated with a specific lesson
        public string Url { get; set; } = null!;
        public string Loai { get; set; } = null!; // "image" hoặc "video"
    }
}
