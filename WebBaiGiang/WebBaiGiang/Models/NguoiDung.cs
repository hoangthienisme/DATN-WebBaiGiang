using System;
using System.Collections.Generic;

namespace WebBaiGiang.Models;

public partial class NguoiDung
{
    public int IdNguoiDung { get; set; }

    public string TenDangNhap { get; set; } = null!;

    public string MatKhau { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string VaiTro { get; set; } = null!;

    public byte TrangThai { get; set; }

    public virtual ICollection<GiangVien> GiangViens { get; set; } = new List<GiangVien>();

    public virtual ICollection<KhoaHoc> KhoaHocNguoiCapNhatNavigations { get; set; } = new List<KhoaHoc>();

    public virtual ICollection<KhoaHoc> KhoaHocNguoiTaoNavigations { get; set; } = new List<KhoaHoc>();

    public virtual ICollection<Khoa> KhoaNguoiCapNhatNavigations { get; set; } = new List<Khoa>();

    public virtual ICollection<Khoa> KhoaNguoiTaoNavigations { get; set; } = new List<Khoa>();

    public virtual ICollection<LopHoc> LopHocNguoiCapNhatNavigations { get; set; } = new List<LopHoc>();

    public virtual ICollection<LopHoc> LopHocNguoiTaoNavigations { get; set; } = new List<LopHoc>();

    public virtual ICollection<SinhVien> SinhViens { get; set; } = new List<SinhVien>();
}
