using System;
using System.Collections.Generic;

namespace WebBaiGiang.Models;

public partial class Course
{
    public int IdCourse { get; set; }

    public int? IdDepartment { get; set; }

    public string CourseName { get; set; } = null!;

    public string? ShortDescription { get; set; }

    public string? FullDescription { get; set; }

    public string? Major { get; set; }

    public string? Image { get; set; }

    public byte? Status { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public virtual ICollection<ClassCourse> ClassCourses { get; set; } = new List<ClassCourse>();

    public virtual User? CreatedByNavigation { get; set; }

    public virtual Department? IdDepartmentNavigation { get; set; }

    public virtual User? UpdatedByNavigation { get; set; }
}
