using System;
using System.Collections.Generic;

namespace WebBaiGiang.Models;

public partial class ChiTietDiemDanh
{
    public int Id { get; set; }

    public int AttendanceId { get; set; }

    public int UsersId { get; set; }

    public string? Status { get; set; }

    public DateTime ThoiGianDiemDanh { get; set; }

    public virtual DiemDanh Attendance { get; set; } = null!;

    public virtual NguoiDung Users { get; set; } = null!;
}
