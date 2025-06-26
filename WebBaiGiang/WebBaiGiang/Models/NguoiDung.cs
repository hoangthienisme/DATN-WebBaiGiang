using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebBaiGiang.Models;

public partial class NguoiDung
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;
    [Required(ErrorMessage = "*Email là bắt buộc")]
    public string? Email { get; set; } = null!;
    [Required(ErrorMessage = "*Mật khẩu là bắt buộc")]
    public string? Password { get; set; } = null!;

    public string? Avatar { get; set; }
    public string? Gender { get; set; }

    public string? Phone { get; set; }

    public string? Role { get; set; }

    public DateTime CreatedDate { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdateDate { get; set; }

    public int? UpdateBy { get; set; }

    public string? ResetPasswordToken { get; set; }

    public DateTime? ResetTokenExpiry { get; set; }
    public bool IsActive { get; set; } = true;
    public virtual ICollection<GiangVienLopHoc> GiangVienLopHocs { get; set; } = new List<GiangVienLopHoc>();
    public virtual ICollection<BinhLuan> BinhLuans { get; set; } = new List<BinhLuan>();

    public virtual ICollection<NopBai> NopBais { get; set; } = new List<NopBai>();

    public virtual ICollection<SinhVienLopHoc> SinhVienLopHocs { get; set; } = new List<SinhVienLopHoc>();
}
