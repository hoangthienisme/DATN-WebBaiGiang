using System;
using System.Collections.Generic;

namespace WebBaiGiang.Models;

public partial class Diem
{
    public int IdDiem { get; set; }

    public int IdSinhVien { get; set; }

    public int IdKhoaHoc { get; set; }

    public double DiemSo { get; set; }

    public string? DanhGia { get; set; }

    public virtual KhoaHoc IdKhoaHocNavigation { get; set; } = null!;

    public virtual SinhVien IdSinhVienNavigation { get; set; } = null!;
}
