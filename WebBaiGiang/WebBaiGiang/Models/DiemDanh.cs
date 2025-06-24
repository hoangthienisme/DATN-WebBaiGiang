using System;
using System.Collections.Generic;

namespace WebBaiGiang.Models;

public partial class DiemDanh
{
    public int Id { get; set; }

    public int ClassId { get; set; }

    public DateTime CreatedDate { get; set; }

    public int? CreatedBy { get; set; }

    public string MaPhieu { get; set; } = null!;

    public bool MoDiemDanh { get; set; }

    public DateTime? ExpiredAt { get; set; }

    public virtual ICollection<ChiTietDiemDanh> ChiTietDiemDanhs { get; set; } = new List<ChiTietDiemDanh>();

    public virtual LopHoc Class { get; set; } = null!;
}
