using System;
using System.Collections.Generic;

namespace WebBaiGiang.Models;

public partial class BaiGiang
{
    public int IdBaiGiang { get; set; }

    public string TieuDe { get; set; } = null!;

    public string NoiDung { get; set; } = null!;

    public string? TepDinhKem { get; set; }

    public int IdLopHoc { get; set; }

    public DateTime NgayDang { get; set; }

    public byte TrangThai { get; set; }

    public virtual LopHoc IdLopHocNavigation { get; set; } = null!;
}
