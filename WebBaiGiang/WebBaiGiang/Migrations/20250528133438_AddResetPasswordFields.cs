using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebBaiGiang.Migrations
{
    /// <inheritdoc />
    public partial class AddResetPasswordFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Khoa",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    UpdateDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    UpdateBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Khoa__3214EC07F7214B85", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NguoiDung",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Password = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Avatar = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Role = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    UpdateDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    UpdateBy = table.Column<int>(type: "int", nullable: true),
                    ResetPasswordToken = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ResetTokenExpiry = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__NguoiDun__3214EC07E5A0C2B4", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ThongTinWeb",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LogoUrl = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    SocialLink = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    UpdateDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    UpdateBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__ThongTin__3214EC073CFA3B97", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HocPhan",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Department_id = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    UpdateDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    UpdateBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__HocPhan__3214EC07520E5980", x => x.Id);
                    table.ForeignKey(
                        name: "FK__HocPhan__Departm__3D5E1FD2",
                        column: x => x.Department_id,
                        principalTable: "Khoa",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "LopHoc",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Subjects_id = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Picture = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    UpdateDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    UpdateBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__LopHoc__3214EC0728E5A9A5", x => x.Id);
                    table.ForeignKey(
                        name: "FK__LopHoc__Subjects__403A8C7D",
                        column: x => x.Subjects_id,
                        principalTable: "HocPhan",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "BaiGiang",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Class_id = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ContentUrl = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    UpdateDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    UpdateBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__BaiGiang__3214EC07CA07DEEA", x => x.Id);
                    table.ForeignKey(
                        name: "FK__BaiGiang__Class___4F7CD00D",
                        column: x => x.Class_id,
                        principalTable: "LopHoc",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "BaiTap",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Class_id = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DueDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__BaiTap__3214EC07210C887D", x => x.Id);
                    table.ForeignKey(
                        name: "FK__BaiTap__Class_id__52593CB8",
                        column: x => x.Class_id,
                        principalTable: "LopHoc",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DanhGia",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Class_id = table.Column<int>(type: "int", nullable: false),
                    Users_id = table.Column<int>(type: "int", nullable: false),
                    Rating = table.Column<int>(type: "int", nullable: true),
                    Comment = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    UpdateDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    UpdateBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__DanhGia__3214EC07BC54173D", x => x.Id);
                    table.ForeignKey(
                        name: "FK__DanhGia__Class_i__619B8048",
                        column: x => x.Class_id,
                        principalTable: "LopHoc",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK__DanhGia__Users_i__628FA481",
                        column: x => x.Users_id,
                        principalTable: "NguoiDung",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DiemDanh",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Class_id = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__DiemDanh__3214EC077D220E2F", x => x.Id);
                    table.ForeignKey(
                        name: "FK__DiemDanh__Class___59063A47",
                        column: x => x.Class_id,
                        principalTable: "LopHoc",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "GiangVien_LopHoc",
                columns: table => new
                {
                    Id_GV = table.Column<int>(type: "int", nullable: false),
                    Id_Class = table.Column<int>(type: "int", nullable: false),
                    AssignedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    RoleInClass = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__GiangVie__E8DC587B0AF328AF", x => new { x.Id_GV, x.Id_Class });
                    table.ForeignKey(
                        name: "FK__GiangVien__Id_Cl__48CFD27E",
                        column: x => x.Id_Class,
                        principalTable: "LopHoc",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK__GiangVien__Id_GV__47DBAE45",
                        column: x => x.Id_GV,
                        principalTable: "NguoiDung",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "LoiMoi",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Class_id = table.Column<int>(type: "int", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Token = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Role = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ExpiresTime = table.Column<DateTime>(type: "datetime", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__LoiMoi__3214EC076619C972", x => x.Id);
                    table.ForeignKey(
                        name: "FK__LoiMoi__Class_id__4CA06362",
                        column: x => x.Class_id,
                        principalTable: "LopHoc",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SinhVien_LopHoc",
                columns: table => new
                {
                    Id_SV = table.Column<int>(type: "int", nullable: false),
                    Id_Class = table.Column<int>(type: "int", nullable: false),
                    JoinDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__SinhVien__E8DC78F0586635CB", x => new { x.Id_SV, x.Id_Class });
                    table.ForeignKey(
                        name: "FK__SinhVien___Id_Cl__440B1D61",
                        column: x => x.Id_Class,
                        principalTable: "LopHoc",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK__SinhVien___Id_SV__4316F928",
                        column: x => x.Id_SV,
                        principalTable: "NguoiDung",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "NopBai",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Test_id = table.Column<int>(type: "int", nullable: false),
                    Users_id = table.Column<int>(type: "int", nullable: false),
                    FileUrl = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    SubmittedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    Point = table.Column<double>(type: "float", nullable: true),
                    FeedBack = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__NopBai__3214EC07EC907CDB", x => x.Id);
                    table.ForeignKey(
                        name: "FK__NopBai__Test_id__5535A963",
                        column: x => x.Test_id,
                        principalTable: "BaiTap",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK__NopBai__Users_id__5629CD9C",
                        column: x => x.Users_id,
                        principalTable: "NguoiDung",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ChiTietDiemDanh",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Attendance_id = table.Column<int>(type: "int", nullable: false),
                    Users_id = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__ChiTietD__3214EC07C7D082B4", x => x.Id);
                    table.ForeignKey(
                        name: "FK__ChiTietDi__Atten__5CD6CB2B",
                        column: x => x.Attendance_id,
                        principalTable: "DiemDanh",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK__ChiTietDi__Users__5DCAEF64",
                        column: x => x.Users_id,
                        principalTable: "NguoiDung",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_BaiGiang_Class_id",
                table: "BaiGiang",
                column: "Class_id");

            migrationBuilder.CreateIndex(
                name: "IX_BaiTap_Class_id",
                table: "BaiTap",
                column: "Class_id");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietDiemDanh_Attendance_id",
                table: "ChiTietDiemDanh",
                column: "Attendance_id");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietDiemDanh_Users_id",
                table: "ChiTietDiemDanh",
                column: "Users_id");

            migrationBuilder.CreateIndex(
                name: "IX_DanhGia_Class_id",
                table: "DanhGia",
                column: "Class_id");

            migrationBuilder.CreateIndex(
                name: "IX_DanhGia_Users_id",
                table: "DanhGia",
                column: "Users_id");

            migrationBuilder.CreateIndex(
                name: "IX_DiemDanh_Class_id",
                table: "DiemDanh",
                column: "Class_id");

            migrationBuilder.CreateIndex(
                name: "IX_GiangVien_LopHoc_Id_Class",
                table: "GiangVien_LopHoc",
                column: "Id_Class");

            migrationBuilder.CreateIndex(
                name: "IX_HocPhan_Department_id",
                table: "HocPhan",
                column: "Department_id");

            migrationBuilder.CreateIndex(
                name: "IX_LoiMoi_Class_id",
                table: "LoiMoi",
                column: "Class_id");

            migrationBuilder.CreateIndex(
                name: "IX_LopHoc_Subjects_id",
                table: "LopHoc",
                column: "Subjects_id");

            migrationBuilder.CreateIndex(
                name: "UQ__NguoiDun__A9D10534FB2EE92F",
                table: "NguoiDung",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NopBai_Test_id",
                table: "NopBai",
                column: "Test_id");

            migrationBuilder.CreateIndex(
                name: "IX_NopBai_Users_id",
                table: "NopBai",
                column: "Users_id");

            migrationBuilder.CreateIndex(
                name: "IX_SinhVien_LopHoc_Id_Class",
                table: "SinhVien_LopHoc",
                column: "Id_Class");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BaiGiang");

            migrationBuilder.DropTable(
                name: "ChiTietDiemDanh");

            migrationBuilder.DropTable(
                name: "DanhGia");

            migrationBuilder.DropTable(
                name: "GiangVien_LopHoc");

            migrationBuilder.DropTable(
                name: "LoiMoi");

            migrationBuilder.DropTable(
                name: "NopBai");

            migrationBuilder.DropTable(
                name: "SinhVien_LopHoc");

            migrationBuilder.DropTable(
                name: "ThongTinWeb");

            migrationBuilder.DropTable(
                name: "DiemDanh");

            migrationBuilder.DropTable(
                name: "BaiTap");

            migrationBuilder.DropTable(
                name: "NguoiDung");

            migrationBuilder.DropTable(
                name: "LopHoc");

            migrationBuilder.DropTable(
                name: "HocPhan");

            migrationBuilder.DropTable(
                name: "Khoa");
        }
    }
}
