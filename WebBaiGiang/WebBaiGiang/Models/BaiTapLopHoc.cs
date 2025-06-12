namespace WebBaiGiang.Models
{
    public class BaiTapLopHoc
    {
        public int BaiTapId { get; set; }
        public BaiTap BaiTap { get; set; } = null!;

        public int LopHocId { get; set; }
        public LopHoc LopHoc { get; set; } = null!;

        public DateTime NgayGiao { get; set; } = DateTime.Now;
    }
}
