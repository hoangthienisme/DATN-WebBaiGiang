using System;
using System.Collections.Generic;

namespace WebBaiGiang.Models;

public partial class LoiMoi
{
    public int Id { get; set; }

    public int ClassId { get; set; }

    public string Email { get; set; } = null!;

    public string Token { get; set; } = null!;

    public string? Role { get; set; }

    public DateTime ExpiresTime { get; set; }

    public DateTime CreatedDate { get; set; }

    public bool IsActive { get; set; }

    public virtual LopHoc Class { get; set; } = null!;
}
