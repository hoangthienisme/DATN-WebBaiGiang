using System;
using System.Collections.Generic;

namespace WebBaiGiang.Models;

public partial class BaiGiang
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string? ContentUrl { get; set; }

    public string? Description { get; set; }

    public DateTime CreatedDate { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdateDate { get; set; }

    public int? UpdateBy { get; set; }
    public virtual ICollection<TaiNguyen> TaiNguyens { get; set; } = new List<TaiNguyen>();
    public virtual ICollection<LopHocBaiGiang> LopHocBaiGiangs { get; set; } = new List<LopHocBaiGiang>();

    public virtual ICollection<Chuong> Chuongs { get; set; } = new List<Chuong>();

}
