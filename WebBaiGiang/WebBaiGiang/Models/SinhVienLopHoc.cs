using System;
using System.Collections.Generic;

namespace WebBaiGiang.Models;

public partial class SinhVienLopHoc
{
    public int IdSv { get; set; }

    public int IdClass { get; set; }

    public DateTime JoinDate { get; set; }

    public bool IsActive { get; set; }

    public virtual LopHoc IdClassNavigation { get; set; } = null!;

    public virtual NguoiDung IdSvNavigation { get; set; } = null!;
}
