using System;
using System.Collections.Generic;

namespace WebBaiGiang.Models;

public partial class GiangVienLopHoc
{
    public int IdGv { get; set; }

    public int IdClass { get; set; }

    public DateTime AssignedDate { get; set; }

    public string? RoleInClass { get; set; }

    public bool IsActive { get; set; }

    public virtual LopHoc IdClassNavigation { get; set; } = null!;

    public virtual NguoiDung IdGvNavigation { get; set; } = null!;
}
