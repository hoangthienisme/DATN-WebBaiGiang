using System;
using System.Collections.Generic;

namespace WebBaiGiang.Models;

public partial class Bai
{
    public int Id { get; set; }

    public int ChuongId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public string? VideoUrl { get; set; }

    public string? Document { get; set; }

    public int SortOrder { get; set; }

    public DateTime? CreatedDate { get; set; }

    public virtual Chuong Chuong { get; set; } = null!;

    public ICollection<TaiNguyen> TaiNguyens { get; set; } = new List<TaiNguyen>();
}
