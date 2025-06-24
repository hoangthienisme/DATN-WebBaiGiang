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

    public virtual DbSet<Bai> Bais { get; set; }

    public virtual DbSet<BaiGiang> BaiGiangs { get; set; }

    public virtual DbSet<BaiTap> BaiTaps { get; set; }

    public virtual DbSet<BaiTapLopHoc> BaiTapLopHocs { get; set; }

    public virtual DbSet<ChiTietDiemDanh> ChiTietDiemDanhs { get; set; }

    public virtual DbSet<Chuong> Chuongs { get; set; }

    public virtual DbSet<DanhGium> DanhGia { get; set; }

    public virtual DbSet<DiemDanh> DiemDanhs { get; set; }

    public virtual DbSet<GiangVienLopHoc> GiangVienLopHocs { get; set; }

    public virtual DbSet<HocPhan> HocPhans { get; set; }

    public virtual DbSet<Khoa> Khoas { get; set; }

    public virtual DbSet<LoiMoi> LoiMois { get; set; }

    public virtual DbSet<LopHoc> LopHocs { get; set; }

    public virtual DbSet<NguoiDung> NguoiDungs { get; set; }

    public virtual DbSet<NopBai> NopBais { get; set; }

    public virtual DbSet<SinhVienLopHoc> SinhVienLopHocs { get; set; }

    public virtual DbSet<ThongTinWeb> ThongTinWebs { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=ZITDAGIA\\ZITDAGIA;Database=WebBaiGiang;Integrated Security=True;Encrypt=True;Trust Server Certificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Bai>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Bai__3214EC07BA7BBC2A");

            entity.ToTable("Bai");

            entity.Property(e => e.ChuongId).HasColumnName("Chuong_Id");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description).HasColumnType("text");
            entity.Property(e => e.Document).HasMaxLength(500);
            entity.Property(e => e.Title).HasMaxLength(255);
            entity.Property(e => e.VideoUrl)
                .HasMaxLength(500)
                .HasColumnName("VideoURL");

            entity.HasOne(d => d.Chuong).WithMany(p => p.Bais)
                .HasForeignKey(d => d.ChuongId)
                .HasConstraintName("FK_Bai_Chuong");
        });

        modelBuilder.Entity<BaiGiang>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__BaiGiang__3214EC0773DAF4B8");

            entity.ToTable("BaiGiang");

            entity.Property(e => e.ContentUrl).HasMaxLength(250);
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.UpdateDate).HasColumnType("datetime");
        });

        modelBuilder.Entity<BaiTap>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__BaiTap__3214EC078BBFE362");

            entity.ToTable("BaiTap");

            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.DueDate).HasColumnType("datetime");
            entity.Property(e => e.Title).HasMaxLength(200);
        });

        modelBuilder.Entity<BaiTapLopHoc>(entity =>
        {
            entity.HasKey(e => new { e.BaiTapId, e.LopHocId });

            entity.HasOne(d => d.BaiTap).WithMany(p => p.BaiTapLopHocs).HasForeignKey(d => d.BaiTapId);

            entity.HasOne(d => d.LopHoc).WithMany(p => p.BaiTapLopHocs).HasForeignKey(d => d.LopHocId);
        });

        modelBuilder.Entity<ChiTietDiemDanh>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ChiTietD__3214EC07AE926B87");

            entity.ToTable("ChiTietDiemDanh");

            entity.Property(e => e.AttendanceId).HasColumnName("Attendance_id");
            entity.Property(e => e.Status).HasMaxLength(10);
            entity.Property(e => e.UsersId).HasColumnName("Users_id");

            entity.HasOne(d => d.Attendance).WithMany(p => p.ChiTietDiemDanhs)
                .HasForeignKey(d => d.AttendanceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ChiTietDi__Atten__656C112C");

            entity.HasOne(d => d.Users).WithMany(p => p.ChiTietDiemDanhs)
                .HasForeignKey(d => d.UsersId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ChiTietDi__Users__66603565");
        });

        modelBuilder.Entity<Chuong>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Chuong__3214EC0760ED416E");

            entity.ToTable("Chuong");

            entity.Property(e => e.BaiGiangId).HasColumnName("BaiGiang_Id");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Title).HasMaxLength(255);

            entity.HasOne(d => d.BaiGiang).WithMany(p => p.Chuongs)
                .HasForeignKey(d => d.BaiGiangId)
                .HasConstraintName("FK_Chuong_BaiGiang");
        });

        modelBuilder.Entity<DanhGium>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__DanhGia__3214EC07358BC092");

            entity.Property(e => e.ClassId).HasColumnName("Class_id");
            entity.Property(e => e.Comment).HasMaxLength(500);
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.UpdateDate).HasColumnType("datetime");
            entity.Property(e => e.UsersId).HasColumnName("Users_id");

            entity.HasOne(d => d.Class).WithMany(p => p.DanhGia)
                .HasForeignKey(d => d.ClassId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DanhGia__Class_i__68487DD7");

            entity.HasOne(d => d.Users).WithMany(p => p.DanhGia)
                .HasForeignKey(d => d.UsersId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DanhGia__Users_i__693CA210");
        });

        modelBuilder.Entity<DiemDanh>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__DiemDanh__3214EC07ED1D8A7D");

            entity.ToTable("DiemDanh");

            entity.Property(e => e.ClassId).HasColumnName("Class_id");
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.MaPhieu).HasDefaultValue("");

            entity.HasOne(d => d.Class).WithMany(p => p.DiemDanhs)
                .HasForeignKey(d => d.ClassId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DiemDanh__Class___6A30C649");
        });

        modelBuilder.Entity<GiangVienLopHoc>(entity =>
        {
            entity.HasKey(e => new { e.IdGv, e.IdClass }).HasName("PK__GiangVie__E8DC587BB2E14558");

            entity.ToTable("GiangVien_LopHoc");

            entity.Property(e => e.IdGv).HasColumnName("Id_GV");
            entity.Property(e => e.IdClass).HasColumnName("Id_Class");
            entity.Property(e => e.AssignedDate).HasColumnType("datetime");
            entity.Property(e => e.RoleInClass).HasMaxLength(20);

            entity.HasOne(d => d.IdClassNavigation).WithMany(p => p.GiangVienLopHocs)
                .HasForeignKey(d => d.IdClass)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__GiangVien__Id_Cl__6B24EA82");

            entity.HasOne(d => d.IdGvNavigation).WithMany(p => p.GiangVienLopHocs)
                .HasForeignKey(d => d.IdGv)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__GiangVien__Id_GV__6C190EBB");
        });

        modelBuilder.Entity<HocPhan>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__HocPhan__3214EC07544CF630");

            entity.ToTable("HocPhan");

            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.DepartmentId).HasColumnName("Department_id");
            entity.Property(e => e.Description)
                .HasMaxLength(250)
                .HasDefaultValue("");
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.UpdateDate).HasColumnType("datetime");

            entity.HasOne(d => d.Department).WithMany(p => p.HocPhans)
                .HasForeignKey(d => d.DepartmentId)
                .HasConstraintName("FK__HocPhan__Departm__6D0D32F4");
        });

        modelBuilder.Entity<Khoa>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Khoa__3214EC0748E72442");

            entity.ToTable("Khoa");

            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(250);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.UpdateDate).HasColumnType("datetime");
        });

        modelBuilder.Entity<LoiMoi>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__LoiMoi__3214EC0743794199");

            entity.ToTable("LoiMoi");

            entity.Property(e => e.ClassId).HasColumnName("Class_id");
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.ExpiresTime).HasColumnType("datetime");
            entity.Property(e => e.Role).HasMaxLength(20);
            entity.Property(e => e.Token).HasMaxLength(100);

            entity.HasOne(d => d.Class).WithMany(p => p.LoiMois)
                .HasForeignKey(d => d.ClassId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__LoiMoi__Class_id__6E01572D");
        });

        modelBuilder.Entity<LopHoc>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__LopHoc__3214EC07D758C3F7");

            entity.ToTable("LopHoc");

            entity.Property(e => e.BaiGiangId).HasColumnName("BaiGiang_id");
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(250);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.KhoaId).HasColumnName("Khoa_id");
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Picture).HasMaxLength(250);
            entity.Property(e => e.SubjectsId).HasColumnName("Subjects_id");
            entity.Property(e => e.UpdateDate).HasColumnType("datetime");

            entity.HasOne(d => d.BaiGiang).WithMany(p => p.LopHocs)
                .HasForeignKey(d => d.BaiGiangId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_LopHoc_BaiGiang");

            entity.HasOne(d => d.Khoa).WithMany(p => p.LopHocs)
                .HasForeignKey(d => d.KhoaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LopHoc_Khoa");

            entity.HasOne(d => d.Subjects).WithMany(p => p.LopHocs)
                .HasForeignKey(d => d.SubjectsId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__LopHoc__Subjects__6EF57B66");
        });

        modelBuilder.Entity<NguoiDung>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__NguoiDun__3214EC07483B5502");

            entity.ToTable("NguoiDung");

            entity.HasIndex(e => e.Email, "UQ__NguoiDun__A9D105347D44460A").IsUnique();

            entity.Property(e => e.Avatar).HasMaxLength(250);
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Password).HasMaxLength(100);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.ResetTokenExpiry).HasColumnType("datetime");
            entity.Property(e => e.Role).HasMaxLength(20);
            entity.Property(e => e.UpdateDate).HasColumnType("datetime");
        });

        modelBuilder.Entity<NopBai>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__NopBai__3214EC070B8487C2");

            entity.ToTable("NopBai");

            entity.Property(e => e.FeedBack).HasMaxLength(500);
            entity.Property(e => e.FileUrl).HasMaxLength(250);
            entity.Property(e => e.SubmittedDate).HasColumnType("datetime");
            entity.Property(e => e.TestId).HasColumnName("Test_id");
            entity.Property(e => e.UsersId).HasColumnName("Users_id");

            entity.HasOne(d => d.Test).WithMany(p => p.NopBais)
                .HasForeignKey(d => d.TestId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__NopBai__Test_id__71D1E811");

            entity.HasOne(d => d.Users).WithMany(p => p.NopBais)
                .HasForeignKey(d => d.UsersId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__NopBai__Users_id__72C60C4A");
        });

        modelBuilder.Entity<SinhVienLopHoc>(entity =>
        {
            entity.HasKey(e => new { e.IdSv, e.IdClass }).HasName("PK__SinhVien__E8DC78F04D25C079");

            entity.ToTable("SinhVien_LopHoc");

            entity.Property(e => e.IdSv).HasColumnName("Id_SV");
            entity.Property(e => e.IdClass).HasColumnName("Id_Class");
            entity.Property(e => e.JoinDate).HasColumnType("datetime");

            entity.HasOne(d => d.IdClassNavigation).WithMany(p => p.SinhVienLopHocs)
                .HasForeignKey(d => d.IdClass)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__SinhVien___Id_Cl__73BA3083");

            entity.HasOne(d => d.IdSvNavigation).WithMany(p => p.SinhVienLopHocs)
                .HasForeignKey(d => d.IdSv)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__SinhVien___Id_SV__74AE54BC");
        });

        modelBuilder.Entity<ThongTinWeb>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ThongTin__3214EC0766380EA8");

            entity.ToTable("ThongTinWeb");

            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.LogoUrl).HasMaxLength(250);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.SocialLink).HasMaxLength(250);
            entity.Property(e => e.UpdateDate).HasColumnType("datetime");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
