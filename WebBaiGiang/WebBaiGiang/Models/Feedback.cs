using System;
using System.Collections.Generic;

namespace WebBaiGiang.Models;

public partial class Feedback
{
    public int IdFeedback { get; set; }

    public int? IdClass { get; set; }

    public int? IdStudent { get; set; }

    public int? Rating { get; set; }

    public string? Comment { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public virtual User? CreatedByNavigation { get; set; }

    public virtual ClassCourse? IdClassNavigation { get; set; }

    public virtual Student? IdStudentNavigation { get; set; }

    public virtual User? UpdatedByNavigation { get; set; }
}
