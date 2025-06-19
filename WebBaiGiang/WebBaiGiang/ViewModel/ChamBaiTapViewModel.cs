using WebBaiGiang.Models;

namespace WebBaiGiang.ViewModel
{
    public class ChamBaiTapViewModel
    {
        public BaiTap BaiTap { get; set; } = null!;
        public List<NopBai> DanhSachNop { get; set; } = new List<NopBai>();
    }
}
