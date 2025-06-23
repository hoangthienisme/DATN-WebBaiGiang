namespace WebBaiGiang.Models
{
    public class TaiNguyen
    {
        public int Id { get; set; }
        public int? BaiGiangId { get; set; }
        public int? BaiId { get; set; }

        public string? Url { get; set; }       // dùng cho link YouTube hoặc tên file
        public string Loai { get; set; } = null!;  // image / video / tailieu / youtube

        public byte[]? Data { get; set; }      // nội dung ảnh, file, v.v.

        public BaiGiang? BaiGiang { get; set; }
        public Bai? Bai { get; set; }
    }
}
