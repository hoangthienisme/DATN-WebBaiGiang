using System;
using System.Collections.Generic;

namespace WebBaiGiang.Models;

public partial class LopHoc
{
    public int IdLopHoc { get; set; }

    public string TenLop { get; set; } = null!;

    public int IdKhoaHoc { get; set; }

    public int IdGiangVien { get; set; }

    public byte TrangThai { get; set; }

    public int NguoiTao { get; set; }

    public DateTime NgayTao { get; set; }

    public int? NguoiCapNhat { get; set; }

    public DateTime? NgayCapNhat { get; set; }

    public virtual ICollection<BaiGiang> BaiGiangs { get; set; } = new List<BaiGiang>();

    public virtual ICollection<BaiTap> BaiTaps { get; set; } = new List<BaiTap>();

    public virtual ICollection<DiemDanh> DiemDanhs { get; set; } = new List<DiemDanh>();

    public virtual GiangVien IdGiangVienNavigation { get; set; } = null!;

    public virtual KhoaHoc IdKhoaHocNavigation { get; set; } = null!;

    public virtual NguoiDung? NguoiCapNhatNavigation { get; set; }

    public virtual NguoiDung NguoiTaoNavigation { get; set; } = null!;
}
