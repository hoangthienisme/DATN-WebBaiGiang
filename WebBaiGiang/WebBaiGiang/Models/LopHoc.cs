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

    public DateTime CreatedDate { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdateDate { get; set; }

    public int? UpdateBy { get; set; }

    public int KhoaId { get; set; }
    public bool IsActive { get; set; } = true;
    public string JoinCode { get; set; } = Guid.NewGuid().ToString("N").Substring(0, 8);
    public virtual ICollection<LopHocBaiGiang> LopHocBaiGiangs { get; set; } = new List<LopHocBaiGiang>();


    public virtual ICollection<BaiTapLopHoc> BaiTapLopHocs { get; set; } = new List<BaiTapLopHoc>();

    public virtual ICollection<GiangVienLopHoc> GiangVienLopHocs { get; set; } = new List<GiangVienLopHoc>();

    public virtual Khoa? Khoa { get; set; }

    public virtual ICollection<LoiMoi> LoiMois { get; set; } = new List<LoiMoi>();

    public virtual ICollection<SinhVienLopHoc> SinhVienLopHocs { get; set; } = new List<SinhVienLopHoc>();

    public virtual HocPhan? Subjects { get; set; }
}