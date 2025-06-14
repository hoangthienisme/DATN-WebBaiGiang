using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebBaiGiang.Migrations
{
    /// <inheritdoc />
    public partial class DeleteDiemDanh : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChiTietDiemDanh");

            migrationBuilder.DropTable(
                name: "DiemDanh");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DiemDanh",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Class_id = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    ExpiredAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MaPhieu = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MoDiemDanh = table.Column<bool>(type: "bit", nullable: false)
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
                name: "ChiTietDiemDanh",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Attendance_id = table.Column<int>(type: "int", nullable: false),
                    Users_id = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    ThoiGianDiemDanh = table.Column<DateTime>(type: "datetime2", nullable: false)
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
                name: "IX_ChiTietDiemDanh_Attendance_id",
                table: "ChiTietDiemDanh",
                column: "Attendance_id");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietDiemDanh_Users_id",
                table: "ChiTietDiemDanh",
                column: "Users_id");

            migrationBuilder.CreateIndex(
                name: "IX_DiemDanh_Class_id",
                table: "DiemDanh",
                column: "Class_id");
        }
    }
}
