using System;
using System.Collections.Generic;

namespace WebBaiGiang.Models;

public partial class User
{
    public int IdUser { get; set; }

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string? Avatar { get; set; }

    public string? PhoneNumber { get; set; }

    public string Role { get; set; } = null!;

    public byte? Status { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public virtual ICollection<Assignment> AssignmentCreatedByNavigations { get; set; } = new List<Assignment>();

    public virtual ICollection<AssignmentSubmission> AssignmentSubmissions { get; set; } = new List<AssignmentSubmission>();

    public virtual ICollection<Assignment> AssignmentUpdatedByNavigations { get; set; } = new List<Assignment>();

    public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();

    public virtual ICollection<ClassCourse> ClassCourseCreatedByNavigations { get; set; } = new List<ClassCourse>();

    public virtual ICollection<ClassCourse> ClassCourseUpdatedByNavigations { get; set; } = new List<ClassCourse>();

    public virtual ICollection<Course> CourseCreatedByNavigations { get; set; } = new List<Course>();

    public virtual ICollection<Course> CourseUpdatedByNavigations { get; set; } = new List<Course>();

    public virtual User? CreatedByNavigation { get; set; }

    public virtual ICollection<Department> DepartmentCreatedByNavigations { get; set; } = new List<Department>();

    public virtual ICollection<Department> DepartmentUpdatedByNavigations { get; set; } = new List<Department>();

    public virtual ICollection<Faculty> FacultyCreatedByNavigations { get; set; } = new List<Faculty>();

    public virtual ICollection<Faculty> FacultyUpdatedByNavigations { get; set; } = new List<Faculty>();

    public virtual ICollection<Feedback> FeedbackCreatedByNavigations { get; set; } = new List<Feedback>();

    public virtual ICollection<Feedback> FeedbackUpdatedByNavigations { get; set; } = new List<Feedback>();

    public virtual ICollection<Grade> Grades { get; set; } = new List<Grade>();

    public virtual ICollection<User> InverseCreatedByNavigation { get; set; } = new List<User>();

    public virtual ICollection<User> InverseUpdatedByNavigation { get; set; } = new List<User>();

    public virtual ICollection<LearningStatus> LearningStatuses { get; set; } = new List<LearningStatus>();

    public virtual ICollection<Lecture> LectureCreatedByNavigations { get; set; } = new List<Lecture>();

    public virtual ICollection<Lecture> LectureUpdatedByNavigations { get; set; } = new List<Lecture>();

    public virtual ICollection<Lecturer> LecturerCreatedByNavigations { get; set; } = new List<Lecturer>();

    public virtual Lecturer? LecturerIdUserNavigation { get; set; }

    public virtual ICollection<Lecturer> LecturerUpdatedByNavigations { get; set; } = new List<Lecturer>();

    public virtual ICollection<StudentClassCourse> StudentClassCourseAddedByNavigations { get; set; } = new List<StudentClassCourse>();

    public virtual ICollection<StudentClassCourse> StudentClassCourseUpdatedByNavigations { get; set; } = new List<StudentClassCourse>();

    public virtual ICollection<Student> StudentCreatedByNavigations { get; set; } = new List<Student>();

    public virtual Student? StudentIdUserNavigation { get; set; }

    public virtual ICollection<Student> StudentUpdatedByNavigations { get; set; } = new List<Student>();

    public virtual User? UpdatedByNavigation { get; set; }

    public virtual ICollection<WebsiteInfo> WebsiteInfoCreatedByNavigations { get; set; } = new List<WebsiteInfo>();

    public virtual ICollection<WebsiteInfo> WebsiteInfoUpdatedByNavigations { get; set; } = new List<WebsiteInfo>();
}
