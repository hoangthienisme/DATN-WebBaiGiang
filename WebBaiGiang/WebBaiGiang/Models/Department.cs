using System;
using System.Collections.Generic;

namespace WebBaiGiang.Models;

public partial class Department
{
    public int IdDepartment { get; set; }

    public string DepartmentName { get; set; } = null!;

    public int? IdFaculty { get; set; }

    public string? Description { get; set; }

    public byte? Status { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public virtual ICollection<Course> Courses { get; set; } = new List<Course>();

    public virtual User? CreatedByNavigation { get; set; }

    public virtual Faculty? IdFacultyNavigation { get; set; }

    public virtual ICollection<Lecturer> Lecturers { get; set; } = new List<Lecturer>();

    public virtual User? UpdatedByNavigation { get; set; }
}
