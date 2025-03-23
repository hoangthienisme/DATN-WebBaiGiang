using System;
using System.Collections.Generic;

namespace WebBaiGiang.Models;

public partial class Lecture
{
    public int IdLecture { get; set; }

    public int? IdClass { get; set; }

    public int? Week { get; set; }

    public string Title { get; set; } = null!;

    public string? Content { get; set; }

    public string? Document { get; set; }

    public byte? Status { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public virtual ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();

    public virtual User? CreatedByNavigation { get; set; }

    public virtual ClassCourse? IdClassNavigation { get; set; }

    public virtual ICollection<LearningStatus> LearningStatuses { get; set; } = new List<LearningStatus>();

    public virtual User? UpdatedByNavigation { get; set; }
}
