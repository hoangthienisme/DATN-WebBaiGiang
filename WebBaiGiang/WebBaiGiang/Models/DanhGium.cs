using System;
using System.Collections.Generic;

namespace WebBaiGiang.Models;

public partial class DanhGium
{
    public int Id { get; set; }

    public int ClassId { get; set; }

    public int UsersId { get; set; }

    public int? Rating { get; set; }

    public string? Comment { get; set; }

    public DateTime CreatedDate { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdateDate { get; set; }

    public int? UpdateBy { get; set; }

    public virtual LopHoc Class { get; set; } = null!;

    public virtual NguoiDung Users { get; set; } = null!;
}
