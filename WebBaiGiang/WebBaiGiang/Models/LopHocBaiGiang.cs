namespace WebBaiGiang.Models
{
    public class LopHocBaiGiang
    {
        public int LopHocId { get; set; }
        public LopHoc LopHoc { get; set; }

        public int BaiGiangId { get; set; }
        public BaiGiang BaiGiang { get; set; }
        public DateTime? AddedDate { get; set; }
    }
}
