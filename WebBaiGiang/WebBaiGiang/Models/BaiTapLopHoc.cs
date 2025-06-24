using System;
using System.Collections.Generic;

namespace WebBaiGiang.Models;

public partial class BaiTapLopHoc
{
    public int BaiTapId { get; set; }

    public int LopHocId { get; set; }

    public DateTime NgayGiao { get; set; }

    public virtual BaiTap BaiTap { get; set; } = null!;

    public virtual LopHoc LopHoc { get; set; } = null!;
}
