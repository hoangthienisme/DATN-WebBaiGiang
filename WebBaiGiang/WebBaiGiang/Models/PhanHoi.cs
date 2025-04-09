using System;
using System.Collections.Generic;

namespace WebBaiGiang.Models;

public partial class PhanHoi
{
    public int IdPhanHoi { get; set; }

    public int IdSinhVien { get; set; }

    public int IdKhoaHoc { get; set; }

    public string NoiDung { get; set; } = null!;

    public DateTime NgayGopY { get; set; }

    public virtual KhoaHoc IdKhoaHocNavigation { get; set; } = null!;

    public virtual SinhVien IdSinhVienNavigation { get; set; } = null!;
}
