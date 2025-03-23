using System;
using System.Collections.Generic;

namespace WebBaiGiang.Models;

public partial class Attendance
{
    public int Id { get; set; }

    public int? IdClass { get; set; }

    public int? IdStudent { get; set; }

    public DateOnly? AttendanceDate { get; set; }

    public byte? Status { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual ClassCourse? IdClassNavigation { get; set; }

    public virtual Student? IdStudentNavigation { get; set; }

    public virtual User? UpdatedByNavigation { get; set; }
}
