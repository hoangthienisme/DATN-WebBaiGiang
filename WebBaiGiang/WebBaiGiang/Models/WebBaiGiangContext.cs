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

    public virtual DbSet<BaiGiang> BaiGiangs { get; set; }

    public virtual DbSet<BaiTap> BaiTaps { get; set; }

    public virtual DbSet<Diem> Diems { get; set; }

    public virtual DbSet<DiemDanh> DiemDanhs { get; set; }

    public virtual DbSet<GiangVien> GiangViens { get; set; }

    public virtual DbSet<Khoa> Khoas { get; set; }

    public virtual DbSet<KhoaHoc> KhoaHocs { get; set; }

    public virtual DbSet<KhoaHocSinhVien> KhoaHocSinhViens { get; set; }

    public virtual DbSet<LopHoc> LopHocs { get; set; }

    public virtual DbSet<NguoiDung> NguoiDungs { get; set; }

    public virtual DbSet<NopBaiTap> NopBaiTaps { get; set; }

    public virtual DbSet<PhanHoi> PhanHois { get; set; }

    public virtual DbSet<SinhVien> SinhViens { get; set; }

    public virtual DbSet<ThongTinWebsite> ThongTinWebsites { get; set; }

    public virtual DbSet<TrangThaiHocTap> TrangThaiHocTaps { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=MSI\\SQLEXPRESS;Database=WebBaiGiang;Trusted_Connection=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BaiGiang>(entity =>
        {
            entity.HasKey(e => e.IdBaiGiang).HasName("PK__BaiGiang__160388B0B408B231");

            entity.ToTable("BaiGiang");

            entity.Property(e => e.IdBaiGiang).HasColumnName("Id_BaiGiang");
            entity.Property(e => e.IdLopHoc).HasColumnName("Id_LopHoc");
            entity.Property(e => e.NgayDang).HasColumnType("datetime");
            entity.Property(e => e.TepDinhKem).HasMaxLength(255);
            entity.Property(e => e.TieuDe).HasMaxLength(200);

            entity.HasOne(d => d.IdLopHocNavigation).WithMany(p => p.BaiGiangs)
                .HasForeignKey(d => d.IdLopHoc)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__BaiGiang__Id_Lop__5441852A");
        });

        modelBuilder.Entity<BaiTap>(entity =>
        {
            entity.HasKey(e => e.IdBaiTap).HasName("PK__BaiTap__C2473EA4F9140372");

            entity.ToTable("BaiTap");

            entity.Property(e => e.IdBaiTap).HasColumnName("Id_BaiTap");
            entity.Property(e => e.HanNop).HasColumnType("datetime");
            entity.Property(e => e.IdLopHoc).HasColumnName("Id_LopHoc");
            entity.Property(e => e.NgayGiao).HasColumnType("datetime");
            entity.Property(e => e.TepDinhKem).HasMaxLength(255);
            entity.Property(e => e.TieuDe).HasMaxLength(200);

            entity.HasOne(d => d.IdLopHocNavigation).WithMany(p => p.BaiTaps)
                .HasForeignKey(d => d.IdLopHoc)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__BaiTap__Id_LopHo__571DF1D5");
        });

        modelBuilder.Entity<Diem>(entity =>
        {
            entity.HasKey(e => e.IdDiem).HasName("PK__Diem__101B4D35F19308A9");

            entity.ToTable("Diem");

            entity.Property(e => e.IdDiem).HasColumnName("Id_Diem");
            entity.Property(e => e.DanhGia).HasMaxLength(255);
            entity.Property(e => e.IdKhoaHoc).HasColumnName("Id_KhoaHoc");
            entity.Property(e => e.IdSinhVien).HasColumnName("Id_SinhVien");

            entity.HasOne(d => d.IdKhoaHocNavigation).WithMany(p => p.Diems)
                .HasForeignKey(d => d.IdKhoaHoc)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Diem__Id_KhoaHoc__628FA481");

            entity.HasOne(d => d.IdSinhVienNavigation).WithMany(p => p.Diems)
                .HasForeignKey(d => d.IdSinhVien)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Diem__Id_SinhVie__619B8048");
        });

        modelBuilder.Entity<DiemDanh>(entity =>
        {
            entity.HasKey(e => e.IdDiemDanh).HasName("PK__DiemDanh__B68D149679E1535E");

            entity.ToTable("DiemDanh");

            entity.Property(e => e.IdDiemDanh).HasColumnName("Id_DiemDanh");
            entity.Property(e => e.GhiChu).HasMaxLength(255);
            entity.Property(e => e.IdLopHoc).HasColumnName("Id_LopHoc");
            entity.Property(e => e.IdSinhVien).HasColumnName("Id_SinhVien");
            entity.Property(e => e.NgayHoc).HasColumnType("datetime");

            entity.HasOne(d => d.IdLopHocNavigation).WithMany(p => p.DiemDanhs)
                .HasForeignKey(d => d.IdLopHoc)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DiemDanh__Id_Lop__5DCAEF64");

            entity.HasOne(d => d.IdSinhVienNavigation).WithMany(p => p.DiemDanhs)
                .HasForeignKey(d => d.IdSinhVien)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DiemDanh__Id_Sin__5EBF139D");
        });

        modelBuilder.Entity<GiangVien>(entity =>
        {
            entity.HasKey(e => e.IdGiangVien).HasName("PK__GiangVie__9DD9CBCF08F56E5B");

            entity.ToTable("GiangVien");

            entity.Property(e => e.IdGiangVien).HasColumnName("Id_GiangVien");
            entity.Property(e => e.DiaChi).HasMaxLength(200);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.HoTen).HasMaxLength(100);
            entity.Property(e => e.IdNguoiDung).HasColumnName("Id_NguoiDung");
            entity.Property(e => e.SoDienThoai).HasMaxLength(15);

            entity.HasOne(d => d.IdNguoiDungNavigation).WithMany(p => p.GiangViens)
                .HasForeignKey(d => d.IdNguoiDung)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__GiangVien__Id_Ng__440B1D61");
        });

        modelBuilder.Entity<Khoa>(entity =>
        {
            entity.HasKey(e => e.IdKhoa).HasName("PK__Khoa__E676870D0B4DAD7A");

            entity.ToTable("Khoa");

            entity.Property(e => e.IdKhoa).HasColumnName("Id_Khoa");
            entity.Property(e => e.MoTa).HasMaxLength(500);
            entity.Property(e => e.NgayCapNhat).HasColumnType("datetime");
            entity.Property(e => e.NgayTao).HasColumnType("datetime");
            entity.Property(e => e.TenKhoa).HasMaxLength(100);

            entity.HasOne(d => d.NguoiCapNhatNavigation).WithMany(p => p.KhoaNguoiCapNhatNavigations)
                .HasForeignKey(d => d.NguoiCapNhat)
                .HasConstraintName("FK__Khoa__NguoiCapNh__3C69FB99");

            entity.HasOne(d => d.NguoiTaoNavigation).WithMany(p => p.KhoaNguoiTaoNavigations)
                .HasForeignKey(d => d.NguoiTao)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Khoa__NguoiTao__3B75D760");
        });

        modelBuilder.Entity<KhoaHoc>(entity =>
        {
            entity.HasKey(e => e.IdKhoaHoc).HasName("PK__KhoaHoc__5C213FF6C8B16DF6");

            entity.ToTable("KhoaHoc");

            entity.Property(e => e.IdKhoaHoc).HasColumnName("Id_KhoaHoc");
            entity.Property(e => e.IdKhoa).HasColumnName("Id_Khoa");
            entity.Property(e => e.MoTa).HasMaxLength(500);
            entity.Property(e => e.NgayCapNhat).HasColumnType("datetime");
            entity.Property(e => e.NgayTao).HasColumnType("datetime");
            entity.Property(e => e.TenKhoaHoc).HasMaxLength(100);

            entity.HasOne(d => d.IdKhoaNavigation).WithMany(p => p.KhoaHocs)
                .HasForeignKey(d => d.IdKhoa)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__KhoaHoc__Id_Khoa__3F466844");

            entity.HasOne(d => d.NguoiCapNhatNavigation).WithMany(p => p.KhoaHocNguoiCapNhatNavigations)
                .HasForeignKey(d => d.NguoiCapNhat)
                .HasConstraintName("FK__KhoaHoc__NguoiCa__412EB0B6");

            entity.HasOne(d => d.NguoiTaoNavigation).WithMany(p => p.KhoaHocNguoiTaoNavigations)
                .HasForeignKey(d => d.NguoiTao)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__KhoaHoc__NguoiTa__403A8C7D");
        });

        modelBuilder.Entity<KhoaHocSinhVien>(entity =>
        {
            entity.HasKey(e => e.IdKhsv).HasName("PK__KhoaHocS__9CEDAE8BCDB5D2E2");

            entity.ToTable("KhoaHocSinhVien");

            entity.Property(e => e.IdKhsv).HasColumnName("Id_KHSV");
            entity.Property(e => e.IdKhoaHoc).HasColumnName("Id_KhoaHoc");
            entity.Property(e => e.IdSinhVien).HasColumnName("Id_SinhVien");
            entity.Property(e => e.IdTrangThai).HasColumnName("Id_TrangThai");
            entity.Property(e => e.NgayThamGia).HasColumnType("datetime");

            entity.HasOne(d => d.IdKhoaHocNavigation).WithMany(p => p.KhoaHocSinhViens)
                .HasForeignKey(d => d.IdKhoaHoc)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__KhoaHocSi__Id_Kh__4F7CD00D");

            entity.HasOne(d => d.IdSinhVienNavigation).WithMany(p => p.KhoaHocSinhViens)
                .HasForeignKey(d => d.IdSinhVien)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__KhoaHocSi__Id_Si__5070F446");

            entity.HasOne(d => d.IdTrangThaiNavigation).WithMany(p => p.KhoaHocSinhViens)
                .HasForeignKey(d => d.IdTrangThai)
                .HasConstraintName("FK__KhoaHocSi__Id_Tr__5165187F");
        });

        modelBuilder.Entity<LopHoc>(entity =>
        {
            entity.HasKey(e => e.IdLopHoc).HasName("PK__LopHoc__621CBC8AE3A56444");

            entity.ToTable("LopHoc");

            entity.Property(e => e.IdLopHoc).HasColumnName("Id_LopHoc");
            entity.Property(e => e.IdGiangVien).HasColumnName("Id_GiangVien");
            entity.Property(e => e.IdKhoaHoc).HasColumnName("Id_KhoaHoc");
            entity.Property(e => e.NgayCapNhat).HasColumnType("datetime");
            entity.Property(e => e.NgayTao).HasColumnType("datetime");
            entity.Property(e => e.TenLop).HasMaxLength(100);

            entity.HasOne(d => d.IdGiangVienNavigation).WithMany(p => p.LopHocs)
                .HasForeignKey(d => d.IdGiangVien)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__LopHoc__Id_Giang__47DBAE45");

            entity.HasOne(d => d.IdKhoaHocNavigation).WithMany(p => p.LopHocs)
                .HasForeignKey(d => d.IdKhoaHoc)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__LopHoc__Id_KhoaH__46E78A0C");

            entity.HasOne(d => d.NguoiCapNhatNavigation).WithMany(p => p.LopHocNguoiCapNhatNavigations)
                .HasForeignKey(d => d.NguoiCapNhat)
                .HasConstraintName("FK__LopHoc__NguoiCap__49C3F6B7");

            entity.HasOne(d => d.NguoiTaoNavigation).WithMany(p => p.LopHocNguoiTaoNavigations)
                .HasForeignKey(d => d.NguoiTao)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__LopHoc__NguoiTao__48CFD27E");
        });

        modelBuilder.Entity<NguoiDung>(entity =>
        {
            entity.HasKey(e => e.IdNguoiDung).HasName("PK__NguoiDun__05F81AD2ED5392ED");

            entity.ToTable("NguoiDung");

            entity.Property(e => e.IdNguoiDung).HasColumnName("Id_NguoiDung");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.MatKhau).HasMaxLength(50);
            entity.Property(e => e.TenDangNhap).HasMaxLength(50);
            entity.Property(e => e.VaiTro).HasMaxLength(20);
        });

        modelBuilder.Entity<NopBaiTap>(entity =>
        {
            entity.HasKey(e => e.IdNopBaiTap).HasName("PK__NopBaiTa__962725168B4677C5");

            entity.ToTable("NopBaiTap");

            entity.Property(e => e.IdNopBaiTap).HasColumnName("Id_NopBaiTap");
            entity.Property(e => e.IdBaiTap).HasColumnName("Id_BaiTap");
            entity.Property(e => e.IdSinhVien).HasColumnName("Id_SinhVien");
            entity.Property(e => e.NgayNop).HasColumnType("datetime");
            entity.Property(e => e.TepNop).HasMaxLength(255);

            entity.HasOne(d => d.IdBaiTapNavigation).WithMany(p => p.NopBaiTaps)
                .HasForeignKey(d => d.IdBaiTap)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__NopBaiTap__Id_Ba__59FA5E80");

            entity.HasOne(d => d.IdSinhVienNavigation).WithMany(p => p.NopBaiTaps)
                .HasForeignKey(d => d.IdSinhVien)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__NopBaiTap__Id_Si__5AEE82B9");
        });

        modelBuilder.Entity<PhanHoi>(entity =>
        {
            entity.HasKey(e => e.IdPhanHoi).HasName("PK__PhanHoi__230F5BE0D4921400");

            entity.ToTable("PhanHoi");

            entity.Property(e => e.IdPhanHoi).HasColumnName("Id_PhanHoi");
            entity.Property(e => e.IdKhoaHoc).HasColumnName("Id_KhoaHoc");
            entity.Property(e => e.IdSinhVien).HasColumnName("Id_SinhVien");
            entity.Property(e => e.NgayGopY).HasColumnType("datetime");

            entity.HasOne(d => d.IdKhoaHocNavigation).WithMany(p => p.PhanHois)
                .HasForeignKey(d => d.IdKhoaHoc)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PhanHoi__Id_Khoa__66603565");

            entity.HasOne(d => d.IdSinhVienNavigation).WithMany(p => p.PhanHois)
                .HasForeignKey(d => d.IdSinhVien)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PhanHoi__Id_Sinh__656C112C");
        });

        modelBuilder.Entity<SinhVien>(entity =>
        {
            entity.HasKey(e => e.IdSinhVien).HasName("PK__SinhVien__2085C04CB502E53D");

            entity.ToTable("SinhVien");

            entity.Property(e => e.IdSinhVien).HasColumnName("Id_SinhVien");
            entity.Property(e => e.DiaChi).HasMaxLength(200);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.HoTen).HasMaxLength(100);
            entity.Property(e => e.IdNguoiDung).HasColumnName("Id_NguoiDung");
            entity.Property(e => e.SoDienThoai).HasMaxLength(15);

            entity.HasOne(d => d.IdNguoiDungNavigation).WithMany(p => p.SinhViens)
                .HasForeignKey(d => d.IdNguoiDung)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__SinhVien__Id_Ngu__4CA06362");
        });

        modelBuilder.Entity<ThongTinWebsite>(entity =>
        {
            entity.HasKey(e => e.IdThongTin).HasName("PK__ThongTin__7F80B85F3345AB02");

            entity.ToTable("ThongTinWebsite");

            entity.Property(e => e.IdThongTin).HasColumnName("Id_ThongTin");
            entity.Property(e => e.DiaChi).HasMaxLength(255);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.Logo).HasMaxLength(255);
            entity.Property(e => e.SoDienThoai).HasMaxLength(20);
            entity.Property(e => e.TenTruong).HasMaxLength(100);
        });

        modelBuilder.Entity<TrangThaiHocTap>(entity =>
        {
            entity.HasKey(e => e.IdTrangThai).HasName("PK__TrangTha__22354EB04309172D");

            entity.ToTable("TrangThaiHocTap");

            entity.Property(e => e.IdTrangThai).HasColumnName("Id_TrangThai");
            entity.Property(e => e.MoTa).HasMaxLength(255);
            entity.Property(e => e.TenTrangThai).HasMaxLength(100);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
