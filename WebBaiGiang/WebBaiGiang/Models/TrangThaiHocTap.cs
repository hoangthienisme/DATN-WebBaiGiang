using System;
using System.Collections.Generic;

namespace WebBaiGiang.Models;

public partial class TrangThaiHocTap
{
    public int IdTrangThai { get; set; }

    public string TenTrangThai { get; set; } = null!;

    public string? MoTa { get; set; }

    public virtual ICollection<KhoaHocSinhVien> KhoaHocSinhViens { get; set; } = new List<KhoaHocSinhVien>();
}
