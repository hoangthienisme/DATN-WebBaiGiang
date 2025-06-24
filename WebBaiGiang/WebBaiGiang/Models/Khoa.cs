using System;
using System.Collections.Generic;

namespace WebBaiGiang.Models;

public partial class Khoa
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime CreatedDate { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdateDate { get; set; }

    public int? UpdateBy { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<HocPhan> HocPhans { get; set; } = new List<HocPhan>();

    public virtual ICollection<LopHoc> LopHocs { get; set; } = new List<LopHoc>();
}
