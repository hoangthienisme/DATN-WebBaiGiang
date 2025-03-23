using System;
using System.Collections.Generic;

namespace WebBaiGiang.Models;

public partial class Faculty
{
    public int IdFaculty { get; set; }

    public string FacultyName { get; set; } = null!;

    public string? Description { get; set; }

    public byte? Status { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public virtual User? CreatedByNavigation { get; set; }

    public virtual ICollection<Department> Departments { get; set; } = new List<Department>();

    public virtual ICollection<Lecturer> Lecturers { get; set; } = new List<Lecturer>();

    public virtual User? UpdatedByNavigation { get; set; }
}
