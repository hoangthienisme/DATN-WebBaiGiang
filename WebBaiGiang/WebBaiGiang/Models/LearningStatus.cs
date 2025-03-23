using System;
using System.Collections.Generic;

namespace WebBaiGiang.Models;

public partial class LearningStatus
{
    public int Id { get; set; }

    public int? IdStudent { get; set; }

    public int? IdLecture { get; set; }

    public byte? Status { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public virtual Lecture? IdLectureNavigation { get; set; }

    public virtual Student? IdStudentNavigation { get; set; }

    public virtual User? UpdatedByNavigation { get; set; }
}
