using System;
using System.Collections.Generic;

namespace WebBaiGiang.Models;

public partial class BaiTap
{
    public int IdBaiTap { get; set; }

    public string TieuDe { get; set; } = null!;

    public string? MoTa { get; set; }

    public DateTime HanNop { get; set; }

    public string? TepDinhKem { get; set; }

    public int IdLopHoc { get; set; }

    public DateTime NgayGiao { get; set; }

    public byte TrangThai { get; set; }

    public virtual LopHoc IdLopHocNavigation { get; set; } = null!;

    public virtual ICollection<NopBaiTap> NopBaiTaps { get; set; } = new List<NopBaiTap>();
}
