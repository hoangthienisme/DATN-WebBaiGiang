using System;
using System.Collections.Generic;

namespace WebBaiGiang.Models;

public partial class BaiTap
{
    public int Id { get; set; }

    public int ClassId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime? DueDate { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedDate { get; set; }

    public int? CreatedBy { get; set; }
    public string? ContentUrl { get; set; }

    public virtual LopHoc Class { get; set; } = null!;

    public virtual ICollection<NopBai> NopBais { get; set; } = new List<NopBai>();
}
