using System;
using System.Collections.Generic;

namespace WebBaiGiang.Models;

public partial class NopBaiTap
{
    public int IdNopBaiTap { get; set; }

    public int IdBaiTap { get; set; }

    public int IdSinhVien { get; set; }

    public string? TepNop { get; set; }

    public DateTime NgayNop { get; set; }

    public double? Diem { get; set; }

    public string? NhanXet { get; set; }

    public virtual BaiTap IdBaiTapNavigation { get; set; } = null!;

    public virtual SinhVien IdSinhVienNavigation { get; set; } = null!;
}
