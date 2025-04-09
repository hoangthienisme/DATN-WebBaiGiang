using System;
using System.Collections.Generic;

namespace WebBaiGiang.Models;

public partial class SinhVien
{
    public int IdSinhVien { get; set; }

    public string HoTen { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? SoDienThoai { get; set; }

    public string? DiaChi { get; set; }

    public byte TrangThai { get; set; }

    public int IdNguoiDung { get; set; }

    public virtual ICollection<DiemDanh> DiemDanhs { get; set; } = new List<DiemDanh>();

    public virtual ICollection<Diem> Diems { get; set; } = new List<Diem>();

    public virtual NguoiDung IdNguoiDungNavigation { get; set; } = null!;

    public virtual ICollection<KhoaHocSinhVien> KhoaHocSinhViens { get; set; } = new List<KhoaHocSinhVien>();

    public virtual ICollection<NopBaiTap> NopBaiTaps { get; set; } = new List<NopBaiTap>();

    public virtual ICollection<PhanHoi> PhanHois { get; set; } = new List<PhanHoi>();
}
