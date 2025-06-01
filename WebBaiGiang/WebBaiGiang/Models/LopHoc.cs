using System;
using System.Collections.Generic;

namespace WebBaiGiang.Models;

public partial class LopHoc
{
    public int Id { get; set; }

    public int SubjectsId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string? Picture { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdateDate { get; set; }

    public int? UpdateBy { get; set; }

    public virtual ICollection<BaiGiang> BaiGiangs { get; set; } = new List<BaiGiang>();

    public virtual ICollection<BaiTap> BaiTaps { get; set; } = new List<BaiTap>();

    public virtual ICollection<DanhGium> DanhGia { get; set; } = new List<DanhGium>();

    public virtual ICollection<DiemDanh> DiemDanhs { get; set; } = new List<DiemDanh>();

    public virtual ICollection<GiangVienLopHoc> GiangVienLopHocs { get; set; } = new List<GiangVienLopHoc>();

    public virtual ICollection<LoiMoi> LoiMois { get; set; } = new List<LoiMoi>();

    public virtual ICollection<SinhVienLopHoc> SinhVienLopHocs { get; set; } = new List<SinhVienLopHoc>();

    public virtual HocPhan? Subjects { get; set; } 
}
