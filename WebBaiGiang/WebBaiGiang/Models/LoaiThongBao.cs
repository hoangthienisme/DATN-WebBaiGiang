using System.ComponentModel.DataAnnotations;
namespace WebBaiGiang.Models
{
    public enum LoaiThongBao
    {   // tb cho bai giang 
        [Display(Name = "Bài giảng mới")]
        BaiGiangMoi,
        [Display(Name = "Cập nhật bài giảng")]
        CapNhatBaiGiang,
        [Display(Name = "Xóa bài giảng")]
        XoaBaiGiang,


        // tb cho lop hoc 
        [Display(Name = "Cập nhật lớp học")]
        CapNhatLop,

        // tb cho cmt 
        [Display(Name = "Bình luận mới")]
        BinhLuanMoi,

        // tb cho bai tap
        [Display(Name = "Bài tập mới")]
        BaiTapMoi,
        [Display(Name = "Cập nhật bài tập")]
        CapNhatBaiTap,
        [Display(Name = "Đã chấm điểm")]
        DaChamDiem,
        [Display(Name = "Xóa bài tập")]
        XoaBaiTap,

        // neu can 
        [Display(Name = "Tài liệu mới")]
        TaiLieuMoi,

        [Display(Name = "Thông báo lớp học")]
        ThongBaoLop,


        // tb cho gv 
        [Display(Name = "Sinh viên nộp bài")]
        SinhVienNopBai,

        [Display(Name = "Sinh viên bình luận")]
        SinhVienBinhLuan,

        [Display(Name = "Sinh viên tham gia lớp")]
        ThamGiaLop
    }
}
