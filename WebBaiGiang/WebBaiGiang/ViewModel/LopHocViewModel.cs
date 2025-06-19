using WebBaiGiang.Models;

namespace WebBaiGiang.ViewModel
{
    public class LopHocViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Picture { get; set; }

        public PhanTrang<BaiGiang> BaiGiangs { get; set; } = default!;
        public PhanTrang<BaiTap> BaiTaps { get; set; } = default!;
    }
}
