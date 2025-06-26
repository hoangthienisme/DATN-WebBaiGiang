using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebBaiGiang.Models;

public partial class ThongTinWeb
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;
    public string? Description { get; set; }

    public string? LogoUrl { get; set; }
    [NotMapped]
    public IFormFile? LogoFile { get; set; }
    public string? FacebookLink { get; set; }
    public string? YoutubeLink { get; set; }
    public string? InstagramLink { get; set; }
    public string? EmailLienHe { get; set; }
    public string? PhoneLienHe { get; set; }
    public string? DiaChi { get; set; }
    public string? TenTruong { get; set; }
    public DateTime? CreatedDate { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdateDate { get; set; }

    public int? UpdateBy { get; set; }
}
