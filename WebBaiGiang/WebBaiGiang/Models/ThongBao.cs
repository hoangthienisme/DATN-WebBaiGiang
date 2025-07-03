using System.ComponentModel.DataAnnotations.Schema;

namespace WebBaiGiang.Models
{
    public class ThongBao
    {
        public int Id { get; set; }

        public int NguoiNhanId { get; set; }

        public string NoiDung { get; set; }

        public string? LienKet { get; set; }

        public bool DaDoc { get; set; } = false;

        public DateTime ThoiGian { get; set; } = DateTime.Now;

        // Thêm dòng này:
        public LoaiThongBao Loai { get; set; }

        [ForeignKey("NguoiNhanId")]
        public virtual NguoiDung NguoiNhan { get; set; }
    }
}
