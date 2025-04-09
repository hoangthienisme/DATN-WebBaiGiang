using System;
using System.Collections.Generic;

namespace WebBaiGiang.Models;

public partial class DiemDanh
{
    public int IdDiemDanh { get; set; }

    public int IdLopHoc { get; set; }

    public int IdSinhVien { get; set; }

    public DateTime NgayHoc { get; set; }

    public bool CoMat { get; set; }

    public string? GhiChu { get; set; }

    public virtual LopHoc IdLopHocNavigation { get; set; } = null!;

    public virtual SinhVien IdSinhVienNavigation { get; set; } = null!;
}
