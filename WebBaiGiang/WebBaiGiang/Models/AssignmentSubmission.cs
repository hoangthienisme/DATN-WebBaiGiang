using System;
using System.Collections.Generic;

namespace WebBaiGiang.Models;

public partial class AssignmentSubmission
{
    public int Id { get; set; }

    public int? IdAssignment { get; set; }

    public int? IdStudent { get; set; }

    public string? SubmittedFile { get; set; }

    public DateTime? SubmissionDate { get; set; }

    public byte? Status { get; set; }

    public decimal? Score { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public virtual Assignment? IdAssignmentNavigation { get; set; }

    public virtual Student? IdStudentNavigation { get; set; }

    public virtual User? UpdatedByNavigation { get; set; }
}
