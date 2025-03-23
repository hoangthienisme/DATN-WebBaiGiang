using System;
using System.Collections.Generic;

namespace WebBaiGiang.Models;

public partial class ClassCourse
{
    public int IdClass { get; set; }

    public int? IdCourse { get; set; }

    public int? IdLecturer { get; set; }

    public string? ShortDescription { get; set; }

    public string? FullDescription { get; set; }

    public string? Semester { get; set; }

    public string? Image { get; set; }

    public byte? Status { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();

    public virtual User? CreatedByNavigation { get; set; }

    public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();

    public virtual ICollection<Grade> Grades { get; set; } = new List<Grade>();

    public virtual Course? IdCourseNavigation { get; set; }

    public virtual Lecturer? IdLecturerNavigation { get; set; }

    public virtual ICollection<Lecture> Lectures { get; set; } = new List<Lecture>();

    public virtual ICollection<StudentClassCourse> StudentClassCourses { get; set; } = new List<StudentClassCourse>();

    public virtual User? UpdatedByNavigation { get; set; }
}
