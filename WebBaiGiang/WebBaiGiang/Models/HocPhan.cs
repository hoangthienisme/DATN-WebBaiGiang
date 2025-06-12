using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebBaiGiang.Models;

public partial class HocPhan
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;
    [Required(ErrorMessage = "Vui lòng chọn Khoa.")]
    public string? Description { get; set; }

    public int? DepartmentId { get; set; }

    public DateTime CreatedDate { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdateDate { get; set; }

    public int? UpdateBy { get; set; }
    public bool IsActive { get; set; } = true;


    public virtual Khoa? Department { get; set; } = null!;

    public virtual ICollection<LopHoc> LopHocs { get; set; } = new List<LopHoc>();
}
