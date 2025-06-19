using System;
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


    public virtual DbSet<Chuong> Chuongs { get; set; }

    public virtual DbSet<DanhGium> DanhGia { get; set; }


    public virtual DbSet<GiangVienLopHoc> GiangVienLopHocs { get; set; }


    public virtual DbSet<HocPhan> HocPhans { get; set; }

    public virtual DbSet<Khoa> Khoas { get; set; }

    public virtual DbSet<LoiMoi> LoiMois { get; set; }

    public virtual DbSet<LopHoc> LopHocs { get; set; }

    public virtual DbSet<NguoiDung> NguoiDungs { get; set; }

    public virtual DbSet<NopBai> NopBais { get; set; }

    public virtual DbSet<SinhVienLopHoc> SinhVienLopHocs { get; set; }
    public virtual DbSet<BaiTapLopHoc> BaiTapLopHocs { get; set; } = null!;

    public virtual DbSet<LopHocBaiGiang> LopHocBaiGiangs { get; set; }
    public virtual DbSet<ThongTinWeb> ThongTinWebs { get; set; }
    public virtual DbSet<TaiNguyen> TaiNguyens { get; set; }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=MSI\\SQLEXPRESS;Database=WebBaiGiang;Trusted_Connection=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Entity: Bai
        modelBuilder.Entity<Bai>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Bai__3214EC079B0ED36A");

            entity.ToTable("Bai");

            entity.Property(e => e.ChuongId).HasColumnName("Chuong_Id");
            entity.Property(e => e.CreatedDate)
                  .HasDefaultValueSql("(getdate())")
                  .HasColumnType("datetime");
            entity.Property(e => e.Description).HasColumnType("nvarchar(max)");
            entity.Property(e => e.Document).HasMaxLength(500);
            entity.Property(e => e.Title).HasMaxLength(255);
            entity.Property(e => e.VideoUrl)
                  .HasMaxLength(500)
                  .HasColumnName("VideoURL");

            entity.HasOne(d => d.Chuong)
                  .WithMany(p => p.Bais)
                  .HasForeignKey(d => d.ChuongId)
                  .HasConstraintName("FK_Bai_Chuong");
        });

        // Entity: BaiGiang
        modelBuilder.Entity<BaiGiang>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__BaiGiang__3214EC07CA07DEEA");

            entity.ToTable("BaiGiang");

            entity.Property(e => e.ContentUrl).HasMaxLength(250);
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.Description).HasColumnType("nvarchar(max)");
            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.UpdateDate).HasColumnType("datetime");

            entity.HasMany(e => e.TaiNguyens)
                  .WithOne(t => t.BaiGiang)
                  .HasForeignKey(t => t.BaiGiangId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Entity: TaiNguyen
        modelBuilder.Entity<TaiNguyen>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.ToTable("TaiNguyen");

            entity.Property(e => e.Url).HasMaxLength(500).IsRequired();
            entity.Property(e => e.Loai).HasMaxLength(50).IsRequired();
        });

        // Entity: BaiTap
        modelBuilder.Entity<BaiTap>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__BaiTap__3214EC07210C887D");

            entity.ToTable("BaiTap");

            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.Description).HasColumnType("nvarchar(max)");
            entity.Property(e => e.DueDate).HasColumnType("datetime");
            entity.Property(e => e.Title).HasMaxLength(200);
        });

        // Entity: BaiTapLopHoc (many-to-many)
        modelBuilder.Entity<BaiTapLopHoc>()
            .HasKey(bl => new { bl.BaiTapId, bl.LopHocId });

        modelBuilder.Entity<BaiTapLopHoc>()
            .HasOne(bl => bl.BaiTap)
            .WithMany(b => b.BaiTapLopHocs)
            .HasForeignKey(bl => bl.BaiTapId);

        modelBuilder.Entity<BaiTapLopHoc>()
            .HasOne(bl => bl.LopHoc)
            .WithMany(l => l.BaiTapLopHocs)
            .HasForeignKey(bl => bl.LopHocId);

        // Entity: Chuong
        modelBuilder.Entity<Chuong>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Chuong__3214EC0750279F2B");

            entity.ToTable("Chuong");

            entity.Property(e => e.BaiGiangId).HasColumnName("BaiGiang_Id");
            entity.Property(e => e.CreatedDate)
                  .HasDefaultValueSql("(getdate())")
                  .HasColumnType("datetime");
            entity.Property(e => e.Title).HasMaxLength(255);

            entity.HasOne(d => d.BaiGiang)
                  .WithMany(p => p.Chuongs)
                  .HasForeignKey(d => d.BaiGiangId)
                  .HasConstraintName("FK_Chuong_BaiGiang");
        });

        // Entity: DanhGium
        modelBuilder.Entity<DanhGium>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__DanhGia__3214EC07BC54173D");

            entity.Property(e => e.ClassId).HasColumnName("Class_id");
            entity.Property(e => e.UsersId).HasColumnName("Users_id");
            entity.Property(e => e.Comment).HasMaxLength(500);
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.UpdateDate).HasColumnType("datetime");

            entity.HasOne(d => d.Class)
                  .WithMany(p => p.DanhGia)
                  .HasForeignKey(d => d.ClassId)
                  .OnDelete(DeleteBehavior.ClientSetNull)
                  .HasConstraintName("FK__DanhGia__Class_i__619B8048");

            entity.HasOne(d => d.Users)
                  .WithMany(p => p.DanhGia)
                  .HasForeignKey(d => d.UsersId)
                  .OnDelete(DeleteBehavior.ClientSetNull)
                  .HasConstraintName("FK__DanhGia__Users_i__628FA481");
        });

        // Entity: GiangVienLopHoc (many-to-many)
        modelBuilder.Entity<GiangVienLopHoc>(entity =>
        {
            entity.HasKey(e => new { e.IdGv, e.IdClass }).HasName("PK__GiangVie__E8DC587B0AF328AF");

            entity.ToTable("GiangVien_LopHoc");

            entity.Property(e => e.IdGv).HasColumnName("Id_GV");
            entity.Property(e => e.IdClass).HasColumnName("Id_Class");
            entity.Property(e => e.AssignedDate).HasColumnType("datetime");
            entity.Property(e => e.RoleInClass).HasMaxLength(20);

            entity.HasOne(d => d.IdClassNavigation)
                  .WithMany(p => p.GiangVienLopHocs)
                  .HasForeignKey(d => d.IdClass)
                  .OnDelete(DeleteBehavior.ClientSetNull)
                  .HasConstraintName("FK__GiangVien__Id_Cl__48CFD27E");

            entity.HasOne(d => d.IdGvNavigation)
                  .WithMany(p => p.GiangVienLopHocs)
                  .HasForeignKey(d => d.IdGv)
                  .OnDelete(DeleteBehavior.ClientSetNull)
                  .HasConstraintName("FK__GiangVien__Id_GV__47DBAE45");
        });

        // Entity: HocPhan
        modelBuilder.Entity<HocPhan>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__HocPhan__3214EC07520E5980");

            entity.ToTable("HocPhan");

            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.UpdateDate).HasColumnType("datetime");
            entity.Property(e => e.Description).HasColumnType("nvarchar(max)");
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.DepartmentId).HasColumnName("Department_id");

            entity.HasOne(d => d.Department)
                  .WithMany(p => p.HocPhans)
                  .HasForeignKey(d => d.DepartmentId)
                  .OnDelete(DeleteBehavior.ClientSetNull)
                  .HasConstraintName("FK__HocPhan__Departm__3D5E1FD2");
        });

        // Entity: Khoa
        modelBuilder.Entity<Khoa>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Khoa__3214EC07F7214B85");

            entity.ToTable("Khoa");

            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.UpdateDate).HasColumnType("datetime");
            entity.Property(e => e.Description).HasColumnType("nvarchar(max)");
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        // Entity: LoiMoi
        modelBuilder.Entity<LoiMoi>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__LoiMoi__3214EC076619C972");

            entity.ToTable("LoiMoi");

            entity.Property(e => e.ClassId).HasColumnName("Class_id");
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.ExpiresTime).HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.Role).HasMaxLength(20);
            entity.Property(e => e.Token).HasMaxLength(100);

            entity.HasOne(d => d.Class)
                  .WithMany(p => p.LoiMois)
                  .HasForeignKey(d => d.ClassId)
                  .OnDelete(DeleteBehavior.ClientSetNull)
                  .HasConstraintName("FK__LoiMoi__Class_id__4CA06362");
        });

        // Entity: LopHoc
        modelBuilder.Entity<LopHoc>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__LopHoc__3214EC0728E5A9A5");

            entity.ToTable("LopHoc");

            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.UpdateDate).HasColumnType("datetime");
            entity.Property(e => e.Description).HasColumnType("nvarchar(max)");
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Picture).HasMaxLength(250);
            entity.Property(e => e.KhoaId).HasColumnName("Khoa_id");
            entity.Property(e => e.SubjectsId).HasColumnName("Subjects_id");

            entity.HasOne(d => d.Khoa)
                  .WithMany(p => p.LopHocs)
                  .HasForeignKey(d => d.KhoaId)
                  .OnDelete(DeleteBehavior.ClientSetNull)
                  .HasConstraintName("FK_LopHoc_Khoa");

            entity.HasOne(d => d.Subjects)
                  .WithMany(p => p.LopHocs)
                  .HasForeignKey(d => d.SubjectsId)
                  .OnDelete(DeleteBehavior.ClientSetNull)
                  .HasConstraintName("FK__LopHoc__Subjects__403A8C7D");
        });

        // Entity: NguoiDung
        modelBuilder.Entity<NguoiDung>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__NguoiDun__3214EC07E5A0C2B4");

            entity.ToTable("NguoiDung");

            entity.HasIndex(e => e.Email, "UQ__NguoiDun__A9D10534FB2EE92F").IsUnique();

            entity.Property(e => e.Avatar).HasMaxLength(250);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Password).HasMaxLength(100);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.Role).HasMaxLength(20);
            entity.Property(e => e.ResetPasswordToken).HasMaxLength(200);
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.UpdateDate).HasColumnType("datetime");
            entity.Property(e => e.ResetTokenExpiry).HasColumnType("datetime");
        });

        // Entity: NopBai
        modelBuilder.Entity<NopBai>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__NopBai__3214EC07EC907CDB");

            entity.ToTable("NopBai");

            entity.Property(e => e.FeedBack).HasMaxLength(500);
            entity.Property(e => e.FileUrl).HasMaxLength(250);
            entity.Property(e => e.SubmittedDate).HasColumnType("datetime");
            entity.Property(e => e.TestId).HasColumnName("Test_id");
            entity.Property(e => e.UsersId).HasColumnName("Users_id");

            entity.HasOne(d => d.Test)
                  .WithMany(p => p.NopBais)
                  .HasForeignKey(d => d.TestId)
                  .OnDelete(DeleteBehavior.ClientSetNull)
                  .HasConstraintName("FK__NopBai__Test_id__5535A963");

            entity.HasOne(d => d.Users)
                  .WithMany(p => p.NopBais)
                  .HasForeignKey(d => d.UsersId)
                  .OnDelete(DeleteBehavior.ClientSetNull)
                  .HasConstraintName("FK__NopBai__Users_id__5629CD9C");
        });

        // Entity: SinhVienLopHoc (many-to-many)
        modelBuilder.Entity<SinhVienLopHoc>(entity =>
        {
            entity.HasKey(e => new { e.IdSv, e.IdClass }).HasName("PK__SinhVien__E8DC78F0586635CB");

            entity.ToTable("SinhVien_LopHoc");

            entity.Property(e => e.IdSv).HasColumnName("Id_SV");
            entity.Property(e => e.IdClass).HasColumnName("Id_Class");
            entity.Property(e => e.JoinDate).HasColumnType("datetime");

            entity.HasOne(d => d.IdClassNavigation)
                  .WithMany(p => p.SinhVienLopHocs)
                  .HasForeignKey(d => d.IdClass)
                  .OnDelete(DeleteBehavior.ClientSetNull)
                  .HasConstraintName("FK__SinhVien___Id_Cl__440B1D61");

            entity.HasOne(d => d.IdSvNavigation)
                  .WithMany(p => p.SinhVienLopHocs)
                  .HasForeignKey(d => d.IdSv)
                  .OnDelete(DeleteBehavior.ClientSetNull)
                  .HasConstraintName("FK__SinhVien___Id_SV__4316F928");
        });
        // lop hoc bai giang ( n-n ) 
        modelBuilder.Entity<LopHocBaiGiang>(entity =>
        {
            // Khóa chính tổng hợp
            entity.HasKey(e => new { e.LopHocId, e.BaiGiangId });

            // Quan hệ với LopHoc
            entity.HasOne(e => e.LopHoc)
                .WithMany(l => l.LopHocBaiGiangs)
                .HasForeignKey(e => e.LopHocId);

            // Quan hệ với BaiGiang
            entity.HasOne(e => e.BaiGiang)
                .WithMany(b => b.LopHocBaiGiangs)
                .HasForeignKey(e => e.BaiGiangId);

            entity.Property(e => e.AddedDate).HasColumnType("datetime");
        });

        // Entity: ThongTinWeb
        modelBuilder.Entity<ThongTinWeb>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ThongTin__3214EC073CFA3B97");

            entity.ToTable("ThongTinWeb");

            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.UpdateDate).HasColumnType("datetime");
            entity.Property(e => e.LogoUrl).HasMaxLength(250);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.SocialLink).HasMaxLength(250);
        });

        OnModelCreatingPartial(modelBuilder);
    }


    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
