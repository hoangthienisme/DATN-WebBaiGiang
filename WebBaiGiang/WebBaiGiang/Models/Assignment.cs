using System;
using System.Collections.Generic;

namespace WebBaiGiang.Models;

public partial class Assignment
{
    public int IdAssignment { get; set; }

    public int? IdLecture { get; set; }

    public string? Content { get; set; }

    public string? Document { get; set; }

    public DateTime? Deadline { get; set; }

    public byte? Status { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public virtual ICollection<AssignmentSubmission> AssignmentSubmissions { get; set; } = new List<AssignmentSubmission>();

    public virtual User? CreatedByNavigation { get; set; }

    public virtual Lecture? IdLectureNavigation { get; set; }

    public virtual User? UpdatedByNavigation { get; set; }
}
