using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebBaiGiang.Migrations
{
    /// <inheritdoc />
    public partial class AddFieldDiemDanh : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MaPhieu",
                table: "DiemDanh",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "MoDiemDanh",
                table: "DiemDanh",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ThoiGianDiemDanh",
                table: "ChiTietDiemDanh",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaPhieu",
                table: "DiemDanh");

            migrationBuilder.DropColumn(
                name: "MoDiemDanh",
                table: "DiemDanh");

            migrationBuilder.DropColumn(
                name: "ThoiGianDiemDanh",
                table: "ChiTietDiemDanh");
        }
    }
}
