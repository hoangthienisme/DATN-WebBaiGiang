namespace WebBaiGiang.Models
{
    public class BinhLuan
    {
        public int Id { get; set; }

        public string NoiDung { get; set; } = null!;

        public DateTime NgayTao { get; set; }

        public int NguoiDungId { get; set; }
        public NguoiDung NguoiDung { get; set; } = null!;

        public int BaiGiangId { get; set; }
        public BaiGiang BaiGiang { get; set; } = null!;
    }
}
