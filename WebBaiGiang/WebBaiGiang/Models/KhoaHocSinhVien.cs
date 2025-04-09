using System;
using System.Collections.Generic;

namespace WebBaiGiang.Models;

public partial class KhoaHocSinhVien
{
    public int IdKhsv { get; set; }

    public int IdKhoaHoc { get; set; }

    public int IdSinhVien { get; set; }

    public DateTime NgayThamGia { get; set; }

    public byte TrangThai { get; set; }

    public int? IdTrangThai { get; set; }

    public virtual KhoaHoc IdKhoaHocNavigation { get; set; } = null!;

    public virtual SinhVien IdSinhVienNavigation { get; set; } = null!;

    public virtual TrangThaiHocTap? IdTrangThaiNavigation { get; set; }
}
