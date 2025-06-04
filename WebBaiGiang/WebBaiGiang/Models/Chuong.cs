using System;
using System.Collections.Generic;

namespace WebBaiGiang.Models;

public partial class Chuong
{
    public int Id { get; set; }

    public int BaiGiangId { get; set; }

    public string Title { get; set; } = null!;

    public int SortOrder { get; set; }

    public DateTime? CreatedDate { get; set; }

    public virtual BaiGiang BaiGiang { get; set; } = null!;

    public virtual ICollection<Bai> Bais { get; set; } = new List<Bai>();
}
