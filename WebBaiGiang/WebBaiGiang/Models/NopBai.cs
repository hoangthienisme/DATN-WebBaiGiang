using System;
using System.Collections.Generic;

namespace WebBaiGiang.Models;

public partial class NopBai
{
    public int Id { get; set; }

    public int TestId { get; set; }

    public int UsersId { get; set; }

    public string? FileUrl { get; set; }

    public DateTime SubmittedDate { get; set; }

    public double? Point { get; set; }

    public string? FeedBack { get; set; }

    public virtual BaiTap Test { get; set; } = null!;

    public virtual NguoiDung Users { get; set; } = null!;
}
