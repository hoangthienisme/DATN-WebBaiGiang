using System;
using System.Collections.Generic;

namespace WebBaiGiang.Models;

public partial class GiangVien
{
    public int IdGiangVien { get; set; }

    public string HoTen { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? SoDienThoai { get; set; }

    public string? DiaChi { get; set; }

    public byte TrangThai { get; set; }

    public int IdNguoiDung { get; set; }

    public int NguoiTao { get; set; }

    public virtual NguoiDung IdNguoiDungNavigation { get; set; } = null!;

    public virtual ICollection<LopHoc> LopHocs { get; set; } = new List<LopHoc>();
}
