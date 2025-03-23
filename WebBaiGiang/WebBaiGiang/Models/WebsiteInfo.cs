using System;
using System.Collections.Generic;

namespace WebBaiGiang.Models;

public partial class WebsiteInfo
{
    public int Id { get; set; }

    public string WebsiteName { get; set; } = null!;

    public string? LogoUrl { get; set; }

    public string? GoogleLink { get; set; }

    public string? FacebookLink { get; set; }

    public string? TwitterLink { get; set; }

    public string? Description { get; set; }

    public byte? Status { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public virtual User? CreatedByNavigation { get; set; }

    public virtual User? UpdatedByNavigation { get; set; }
}
