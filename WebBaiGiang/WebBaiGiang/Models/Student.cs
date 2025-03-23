using System;
using System.Collections.Generic;

namespace WebBaiGiang.Models;

public partial class Student
{
    public int IdStudent { get; set; }

    public int? IdUser { get; set; }

    public string StudentCode { get; set; } = null!;

    public byte? Status { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public virtual ICollection<AssignmentSubmission> AssignmentSubmissions { get; set; } = new List<AssignmentSubmission>();

    public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();

    public virtual User? CreatedByNavigation { get; set; }

    public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();

    public virtual ICollection<Grade> Grades { get; set; } = new List<Grade>();

    public virtual User? IdUserNavigation { get; set; }

    public virtual ICollection<LearningStatus> LearningStatuses { get; set; } = new List<LearningStatus>();

    public virtual ICollection<StudentClassCourse> StudentClassCourses { get; set; } = new List<StudentClassCourse>();

    public virtual User? UpdatedByNavigation { get; set; }
}
