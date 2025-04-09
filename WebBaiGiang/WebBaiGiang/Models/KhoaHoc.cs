using System;
using System.Collections.Generic;

namespace WebBaiGiang.Models;

public partial class KhoaHoc
{
    public int IdKhoaHoc { get; set; }

    public string TenKhoaHoc { get; set; } = null!;

    public string? MoTa { get; set; }

    public byte TrangThai { get; set; }

    public int IdKhoa { get; set; }

    public int NguoiTao { get; set; }

    public DateTime NgayTao { get; set; }

    public int? NguoiCapNhat { get; set; }

    public DateTime? NgayCapNhat { get; set; }

    public virtual ICollection<Diem> Diems { get; set; } = new List<Diem>();

    public virtual Khoa IdKhoaNavigation { get; set; } = null!;

    public virtual ICollection<KhoaHocSinhVien> KhoaHocSinhViens { get; set; } = new List<KhoaHocSinhVien>();

    public virtual ICollection<LopHoc> LopHocs { get; set; } = new List<LopHoc>();

    public virtual NguoiDung? NguoiCapNhatNavigation { get; set; }

    public virtual NguoiDung NguoiTaoNavigation { get; set; } = null!;

    public virtual ICollection<PhanHoi> PhanHois { get; set; } = new List<PhanHoi>();
}
