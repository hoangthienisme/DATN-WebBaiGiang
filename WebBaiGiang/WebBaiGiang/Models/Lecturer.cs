using System;
using System.Collections.Generic;

namespace WebBaiGiang.Models;

public partial class Lecturer
{
    public int IdLecturer { get; set; }

    public int? IdUser { get; set; }

    public int? IdFaculty { get; set; }

    public int? IdDepartment { get; set; }

    public string? Description { get; set; }

    public byte? Status { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public virtual ICollection<ClassCourse> ClassCourses { get; set; } = new List<ClassCourse>();

    public virtual User? CreatedByNavigation { get; set; }

    public virtual Department? IdDepartmentNavigation { get; set; }

    public virtual Faculty? IdFacultyNavigation { get; set; }

    public virtual User? IdUserNavigation { get; set; }

    public virtual User? UpdatedByNavigation { get; set; }
}
