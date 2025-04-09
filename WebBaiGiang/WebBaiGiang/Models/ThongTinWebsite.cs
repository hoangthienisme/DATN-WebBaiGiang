using System;
using System.Collections.Generic;

namespace WebBaiGiang.Models;

public partial class ThongTinWebsite
{
    public int IdThongTin { get; set; }

    public string TenTruong { get; set; } = null!;

    public string DiaChi { get; set; } = null!;

    public string? SoDienThoai { get; set; }

    public string? Email { get; set; }

    public string? Logo { get; set; }

    public string? MoTa { get; set; }
}
