using WebBaiGiang.Models;

namespace WebBaiGiang.ViewModel
{
    public class ChiTietBaiTapViewModel
    {
        public BaiTap BaiTap { get; set; } = null!;
        public NopBai? NopBai { get; set; } // Có thể null nếu chưa nộp
    }
}
