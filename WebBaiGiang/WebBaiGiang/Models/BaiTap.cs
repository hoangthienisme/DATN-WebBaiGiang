using System;
using System.Collections.Generic;

namespace WebBaiGiang.Models;

public partial class BaiTap
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }
    public double? MaxPoint { get; set; }

    public DateTime? DueDate { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedDate { get; set; }

    public int? CreatedBy { get; set; }
    public string? ContentUrl { get; set; }

    public virtual ICollection<BaiTapLopHoc> BaiTapLopHocs { get; set; } = new List<BaiTapLopHoc>();

    public virtual ICollection<NopBai> NopBais { get; set; } = new List<NopBai>();
}
