using System;
using System.Collections.Generic;

namespace WebBaiGiang.Models;

public partial class Khoa
{
    public int IdKhoa { get; set; }

    public string TenKhoa { get; set; } = null!;

    public string? MoTa { get; set; }

    public byte TrangThai { get; set; }

    public int NguoiTao { get; set; }

    public DateTime NgayTao { get; set; }

    public int? NguoiCapNhat { get; set; }

    public DateTime? NgayCapNhat { get; set; }

    public virtual ICollection<KhoaHoc> KhoaHocs { get; set; } = new List<KhoaHoc>();

    public virtual NguoiDung? NguoiCapNhatNavigation { get; set; }

    public virtual NguoiDung NguoiTaoNavigation { get; set; } = null!;
}
