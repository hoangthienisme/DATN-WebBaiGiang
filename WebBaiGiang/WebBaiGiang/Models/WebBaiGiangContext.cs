using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace WebBaiGiang.Models;

public partial class WebBaiGiangContext : DbContext
{
    public WebBaiGiangContext()
    {
    }

    public WebBaiGiangContext(DbContextOptions<WebBaiGiangContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Assignment> Assignments { get; set; }

    public virtual DbSet<AssignmentSubmission> AssignmentSubmissions { get; set; }

    public virtual DbSet<Attendance> Attendances { get; set; }

    public virtual DbSet<ClassCourse> ClassCourses { get; set; }

    public virtual DbSet<Course> Courses { get; set; }

    public virtual DbSet<Department> Departments { get; set; }

    public virtual DbSet<Faculty> Faculties { get; set; }

    public virtual DbSet<Feedback> Feedbacks { get; set; }

    public virtual DbSet<Grade> Grades { get; set; }

    public virtual DbSet<LearningStatus> LearningStatuses { get; set; }

    public virtual DbSet<Lecture> Lectures { get; set; }

    public virtual DbSet<Lecturer> Lecturers { get; set; }

    public virtual DbSet<Student> Students { get; set; }

    public virtual DbSet<StudentClassCourse> StudentClassCourses { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<WebsiteInfo> WebsiteInfos { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=MSI\\SQLEXPRESS;Database=WebBaiGiang;Trusted_Connection=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Assignment>(entity =>
        {
            entity.HasKey(e => e.IdAssignment).HasName("PK__Assignme__A65CBCFA2649B08E");

            entity.ToTable("Assignment");

            entity.Property(e => e.IdAssignment).HasColumnName("Id_Assignment");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Deadline).HasColumnType("datetime");
            entity.Property(e => e.Document).HasMaxLength(255);
            entity.Property(e => e.IdLecture).HasColumnName("Id_Lecture");
            entity.Property(e => e.Status).HasDefaultValue((byte)1);
            entity.Property(e => e.UpdatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.AssignmentCreatedByNavigations)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK__Assignmen__Creat__3587F3E0");

            entity.HasOne(d => d.IdLectureNavigation).WithMany(p => p.Assignments)
                .HasForeignKey(d => d.IdLecture)
                .HasConstraintName("FK__Assignmen__Id_Le__3493CFA7");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany(p => p.AssignmentUpdatedByNavigations)
                .HasForeignKey(d => d.UpdatedBy)
                .HasConstraintName("FK__Assignmen__Updat__367C1819");
        });

        modelBuilder.Entity<AssignmentSubmission>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Assignme__3214EC074DDC3E61");

            entity.ToTable("AssignmentSubmission");

            entity.Property(e => e.IdAssignment).HasColumnName("Id_Assignment");
            entity.Property(e => e.IdStudent).HasColumnName("Id_Student");
            entity.Property(e => e.Score).HasColumnType("decimal(4, 2)");
            entity.Property(e => e.Status).HasDefaultValue((byte)0);
            entity.Property(e => e.SubmissionDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.SubmittedFile).HasMaxLength(255);
            entity.Property(e => e.UpdatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.IdAssignmentNavigation).WithMany(p => p.AssignmentSubmissions)
                .HasForeignKey(d => d.IdAssignment)
                .HasConstraintName("FK__Assignmen__Id_As__3C34F16F");

            entity.HasOne(d => d.IdStudentNavigation).WithMany(p => p.AssignmentSubmissions)
                .HasForeignKey(d => d.IdStudent)
                .HasConstraintName("FK__Assignmen__Id_St__3D2915A8");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany(p => p.AssignmentSubmissions)
                .HasForeignKey(d => d.UpdatedBy)
                .HasConstraintName("FK__Assignmen__Updat__3E1D39E1");
        });

        modelBuilder.Entity<Attendance>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Attendan__3214EC075D1DA14B");

            entity.ToTable("Attendance");

            entity.Property(e => e.IdClass).HasColumnName("Id_Class");
            entity.Property(e => e.IdStudent).HasColumnName("Id_Student");
            entity.Property(e => e.Status).HasDefaultValue((byte)0);
            entity.Property(e => e.UpdatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.IdClassNavigation).WithMany(p => p.Attendances)
                .HasForeignKey(d => d.IdClass)
                .HasConstraintName("FK__Attendanc__Id_Cl__2739D489");

            entity.HasOne(d => d.IdStudentNavigation).WithMany(p => p.Attendances)
                .HasForeignKey(d => d.IdStudent)
                .HasConstraintName("FK__Attendanc__Id_St__282DF8C2");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany(p => p.Attendances)
                .HasForeignKey(d => d.UpdatedBy)
                .HasConstraintName("FK__Attendanc__Updat__29221CFB");
        });

        modelBuilder.Entity<ClassCourse>(entity =>
        {
            entity.HasKey(e => e.IdClass).HasName("PK__ClassCou__E30692FE89D001A1");

            entity.ToTable("ClassCourse");

            entity.Property(e => e.IdClass).HasColumnName("Id_Class");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IdCourse).HasColumnName("Id_Course");
            entity.Property(e => e.IdLecturer).HasColumnName("Id_Lecturer");
            entity.Property(e => e.Image).HasMaxLength(255);
            entity.Property(e => e.Semester).HasMaxLength(20);
            entity.Property(e => e.ShortDescription).HasMaxLength(200);
            entity.Property(e => e.Status).HasDefaultValue((byte)0);
            entity.Property(e => e.UpdatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.ClassCourseCreatedByNavigations)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK__ClassCour__Creat__01142BA1");

            entity.HasOne(d => d.IdCourseNavigation).WithMany(p => p.ClassCourses)
                .HasForeignKey(d => d.IdCourse)
                .HasConstraintName("FK__ClassCour__Id_Co__7F2BE32F");

            entity.HasOne(d => d.IdLecturerNavigation).WithMany(p => p.ClassCourses)
                .HasForeignKey(d => d.IdLecturer)
                .HasConstraintName("FK__ClassCour__Id_Le__00200768");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany(p => p.ClassCourseUpdatedByNavigations)
                .HasForeignKey(d => d.UpdatedBy)
                .HasConstraintName("FK__ClassCour__Updat__02084FDA");
        });

        modelBuilder.Entity<Course>(entity =>
        {
            entity.HasKey(e => e.IdCourse).HasName("PK__Course__A3EA21073638E3F5");

            entity.ToTable("Course");

            entity.Property(e => e.IdCourse).HasColumnName("Id_Course");
            entity.Property(e => e.CourseName).HasMaxLength(100);
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IdDepartment).HasColumnName("Id_Department");
            entity.Property(e => e.Image).HasMaxLength(255);
            entity.Property(e => e.Major).HasMaxLength(100);
            entity.Property(e => e.ShortDescription).HasMaxLength(200);
            entity.Property(e => e.Status).HasDefaultValue((byte)1);
            entity.Property(e => e.UpdatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.CourseCreatedByNavigations)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK__Course__CreatedB__787EE5A0");

            entity.HasOne(d => d.IdDepartmentNavigation).WithMany(p => p.Courses)
                .HasForeignKey(d => d.IdDepartment)
                .HasConstraintName("FK__Course__Id_Depar__778AC167");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany(p => p.CourseUpdatedByNavigations)
                .HasForeignKey(d => d.UpdatedBy)
                .HasConstraintName("FK__Course__UpdatedB__797309D9");
        });

        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasKey(e => e.IdDepartment).HasName("PK__Departme__8AA7EAD4DF73C21E");

            entity.ToTable("Department");

            entity.Property(e => e.IdDepartment).HasColumnName("Id_Department");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DepartmentName).HasMaxLength(100);
            entity.Property(e => e.IdFaculty).HasColumnName("Id_Faculty");
            entity.Property(e => e.Status).HasDefaultValue((byte)1);
            entity.Property(e => e.UpdatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.DepartmentCreatedByNavigations)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK__Departmen__Creat__5CD6CB2B");

            entity.HasOne(d => d.IdFacultyNavigation).WithMany(p => p.Departments)
                .HasForeignKey(d => d.IdFaculty)
                .HasConstraintName("FK__Departmen__Id_Fa__5BE2A6F2");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany(p => p.DepartmentUpdatedByNavigations)
                .HasForeignKey(d => d.UpdatedBy)
                .HasConstraintName("FK__Departmen__Updat__5DCAEF64");
        });

        modelBuilder.Entity<Faculty>(entity =>
        {
            entity.HasKey(e => e.IdFaculty).HasName("PK__Faculty__55CA3D775A2320AE");

            entity.ToTable("Faculty");

            entity.Property(e => e.IdFaculty).HasColumnName("Id_Faculty");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.FacultyName).HasMaxLength(100);
            entity.Property(e => e.Status).HasDefaultValue((byte)1);
            entity.Property(e => e.UpdatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.FacultyCreatedByNavigations)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK__Faculty__Created__5535A963");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany(p => p.FacultyUpdatedByNavigations)
                .HasForeignKey(d => d.UpdatedBy)
                .HasConstraintName("FK__Faculty__Updated__5629CD9C");
        });

        modelBuilder.Entity<Feedback>(entity =>
        {
            entity.HasKey(e => e.IdFeedback).HasName("PK__Feedback__3DA0483FD50700A6");

            entity.ToTable("Feedback");

            entity.Property(e => e.IdFeedback).HasColumnName("Id_Feedback");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IdClass).HasColumnName("Id_Class");
            entity.Property(e => e.IdStudent).HasColumnName("Id_Student");
            entity.Property(e => e.UpdatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.FeedbackCreatedByNavigations)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK__Feedback__Create__2180FB33");

            entity.HasOne(d => d.IdClassNavigation).WithMany(p => p.Feedbacks)
                .HasForeignKey(d => d.IdClass)
                .HasConstraintName("FK__Feedback__Id_Cla__1F98B2C1");

            entity.HasOne(d => d.IdStudentNavigation).WithMany(p => p.Feedbacks)
                .HasForeignKey(d => d.IdStudent)
                .HasConstraintName("FK__Feedback__Id_Stu__208CD6FA");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany(p => p.FeedbackUpdatedByNavigations)
                .HasForeignKey(d => d.UpdatedBy)
                .HasConstraintName("FK__Feedback__Update__22751F6C");
        });

        modelBuilder.Entity<Grade>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Grade__3214EC0791B7198C");

            entity.ToTable("Grade");

            entity.Property(e => e.GradeType).HasMaxLength(50);
            entity.Property(e => e.IdClass).HasColumnName("Id_Class");
            entity.Property(e => e.IdStudent).HasColumnName("Id_Student");
            entity.Property(e => e.Score).HasColumnType("decimal(4, 2)");
            entity.Property(e => e.UpdatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.IdClassNavigation).WithMany(p => p.Grades)
                .HasForeignKey(d => d.IdClass)
                .HasConstraintName("FK__Grade__Id_Class__2CF2ADDF");

            entity.HasOne(d => d.IdStudentNavigation).WithMany(p => p.Grades)
                .HasForeignKey(d => d.IdStudent)
                .HasConstraintName("FK__Grade__Id_Studen__2DE6D218");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany(p => p.Grades)
                .HasForeignKey(d => d.UpdatedBy)
                .HasConstraintName("FK__Grade__UpdatedBy__2EDAF651");
        });

        modelBuilder.Entity<LearningStatus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Learning__3214EC0711679E8C");

            entity.ToTable("LearningStatus");

            entity.Property(e => e.IdLecture).HasColumnName("Id_Lecture");
            entity.Property(e => e.IdStudent).HasColumnName("Id_Student");
            entity.Property(e => e.Status).HasDefaultValue((byte)0);
            entity.Property(e => e.UpdatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.IdLectureNavigation).WithMany(p => p.LearningStatuses)
                .HasForeignKey(d => d.IdLecture)
                .HasConstraintName("FK__LearningS__Id_Le__18EBB532");

            entity.HasOne(d => d.IdStudentNavigation).WithMany(p => p.LearningStatuses)
                .HasForeignKey(d => d.IdStudent)
                .HasConstraintName("FK__LearningS__Id_St__17F790F9");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany(p => p.LearningStatuses)
                .HasForeignKey(d => d.UpdatedBy)
                .HasConstraintName("FK__LearningS__Updat__19DFD96B");
        });

        modelBuilder.Entity<Lecture>(entity =>
        {
            entity.HasKey(e => e.IdLecture).HasName("PK__Lecture__F3DDADED8A6782F5");

            entity.ToTable("Lecture");

            entity.Property(e => e.IdLecture).HasColumnName("Id_Lecture");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Document).HasMaxLength(255);
            entity.Property(e => e.IdClass).HasColumnName("Id_Class");
            entity.Property(e => e.Status).HasDefaultValue((byte)1);
            entity.Property(e => e.Title).HasMaxLength(255);
            entity.Property(e => e.UpdatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.LectureCreatedByNavigations)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK__Lecture__Created__123EB7A3");

            entity.HasOne(d => d.IdClassNavigation).WithMany(p => p.Lectures)
                .HasForeignKey(d => d.IdClass)
                .HasConstraintName("FK__Lecture__Id_Clas__114A936A");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany(p => p.LectureUpdatedByNavigations)
                .HasForeignKey(d => d.UpdatedBy)
                .HasConstraintName("FK__Lecture__Updated__1332DBDC");
        });

        modelBuilder.Entity<Lecturer>(entity =>
        {
            entity.HasKey(e => e.IdLecturer).HasName("PK__Lecturer__55C124802943179B");

            entity.ToTable("Lecturer");

            entity.HasIndex(e => e.IdUser, "UQ__Lecturer__D03DEDCA2B23F079").IsUnique();

            entity.Property(e => e.IdLecturer).HasColumnName("Id_Lecturer");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IdDepartment).HasColumnName("Id_Department");
            entity.Property(e => e.IdFaculty).HasColumnName("Id_Faculty");
            entity.Property(e => e.IdUser).HasColumnName("Id_User");
            entity.Property(e => e.Status).HasDefaultValue((byte)1);
            entity.Property(e => e.UpdatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.LecturerCreatedByNavigations)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK__Lecturer__Create__6754599E");

            entity.HasOne(d => d.IdDepartmentNavigation).WithMany(p => p.Lecturers)
                .HasForeignKey(d => d.IdDepartment)
                .HasConstraintName("FK__Lecturer__Id_Dep__66603565");

            entity.HasOne(d => d.IdFacultyNavigation).WithMany(p => p.Lecturers)
                .HasForeignKey(d => d.IdFaculty)
                .HasConstraintName("FK__Lecturer__Id_Fac__656C112C");

            entity.HasOne(d => d.IdUserNavigation).WithOne(p => p.LecturerIdUserNavigation)
                .HasForeignKey<Lecturer>(d => d.IdUser)
                .HasConstraintName("FK__Lecturer__Id_Use__6477ECF3");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany(p => p.LecturerUpdatedByNavigations)
                .HasForeignKey(d => d.UpdatedBy)
                .HasConstraintName("FK__Lecturer__Update__68487DD7");
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasKey(e => e.IdStudent).HasName("PK__Student__B7B22F016DE80704");

            entity.ToTable("Student");

            entity.HasIndex(e => e.StudentCode, "UQ__Student__1FC88604BE8739B2").IsUnique();

            entity.HasIndex(e => e.IdUser, "UQ__Student__D03DEDCA68F31C0E").IsUnique();

            entity.Property(e => e.IdStudent).HasColumnName("Id_Student");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IdUser).HasColumnName("Id_User");
            entity.Property(e => e.Status).HasDefaultValue((byte)1);
            entity.Property(e => e.StudentCode).HasMaxLength(20);
            entity.Property(e => e.UpdatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.StudentCreatedByNavigations)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK__Student__Created__70DDC3D8");

            entity.HasOne(d => d.IdUserNavigation).WithOne(p => p.StudentIdUserNavigation)
                .HasForeignKey<Student>(d => d.IdUser)
                .HasConstraintName("FK__Student__Id_User__6FE99F9F");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany(p => p.StudentUpdatedByNavigations)
                .HasForeignKey(d => d.UpdatedBy)
                .HasConstraintName("FK__Student__Updated__71D1E811");
        });

        modelBuilder.Entity<StudentClassCourse>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__StudentC__3214EC0793FC3B23");

            entity.ToTable("StudentClassCourse");

            entity.Property(e => e.AddedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IdClass).HasColumnName("Id_Class");
            entity.Property(e => e.IdStudent).HasColumnName("Id_Student");
            entity.Property(e => e.Status).HasDefaultValue((byte)0);
            entity.Property(e => e.UpdatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.AddedByNavigation).WithMany(p => p.StudentClassCourseAddedByNavigations)
                .HasForeignKey(d => d.AddedBy)
                .HasConstraintName("FK__StudentCl__Added__09A971A2");

            entity.HasOne(d => d.IdClassNavigation).WithMany(p => p.StudentClassCourses)
                .HasForeignKey(d => d.IdClass)
                .HasConstraintName("FK__StudentCl__Id_Cl__07C12930");

            entity.HasOne(d => d.IdStudentNavigation).WithMany(p => p.StudentClassCourses)
                .HasForeignKey(d => d.IdStudent)
                .HasConstraintName("FK__StudentCl__Id_St__08B54D69");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany(p => p.StudentClassCourseUpdatedByNavigations)
                .HasForeignKey(d => d.UpdatedBy)
                .HasConstraintName("FK__StudentCl__Updat__0A9D95DB");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.IdUser).HasName("PK__Users__D03DEDCB075E5D4C");

            entity.HasIndex(e => e.Email, "UQ__Users__A9D10534F0F0AF78").IsUnique();

            entity.Property(e => e.IdUser).HasColumnName("Id_User");
            entity.Property(e => e.Avatar).HasMaxLength(255);
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.Password).HasMaxLength(255);
            entity.Property(e => e.PhoneNumber).HasMaxLength(15);
            entity.Property(e => e.Role).HasMaxLength(20);
            entity.Property(e => e.Status).HasDefaultValue((byte)1);
            entity.Property(e => e.UpdatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.InverseCreatedByNavigation)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK__Users__CreatedBy__4E88ABD4");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany(p => p.InverseUpdatedByNavigation)
                .HasForeignKey(d => d.UpdatedBy)
                .HasConstraintName("FK__Users__UpdatedBy__4F7CD00D");
        });

        modelBuilder.Entity<WebsiteInfo>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__WebsiteI__3214EC07273A0EFA");

            entity.ToTable("WebsiteInfo");

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.FacebookLink).HasMaxLength(255);
            entity.Property(e => e.GoogleLink).HasMaxLength(255);
            entity.Property(e => e.LogoUrl).HasMaxLength(255);
            entity.Property(e => e.Status).HasDefaultValue((byte)1);
            entity.Property(e => e.TwitterLink).HasMaxLength(255);
            entity.Property(e => e.UpdatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.WebsiteName).HasMaxLength(100);

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.WebsiteInfoCreatedByNavigations)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK__WebsiteIn__Creat__43D61337");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany(p => p.WebsiteInfoUpdatedByNavigations)
                .HasForeignKey(d => d.UpdatedBy)
                .HasConstraintName("FK__WebsiteIn__Updat__44CA3770");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
