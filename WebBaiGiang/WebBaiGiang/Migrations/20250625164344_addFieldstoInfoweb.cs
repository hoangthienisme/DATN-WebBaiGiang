using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebBaiGiang.Migrations
{
    /// <inheritdoc />
    public partial class addFieldstoInfoweb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "ThongTinWeb",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DiaChi",
                table: "ThongTinWeb",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmailLienHe",
                table: "ThongTinWeb",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhoneLienHe",
                table: "ThongTinWeb",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TenTruong",
                table: "ThongTinWeb",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "ThongTinWeb");

            migrationBuilder.DropColumn(
                name: "DiaChi",
                table: "ThongTinWeb");

            migrationBuilder.DropColumn(
                name: "EmailLienHe",
                table: "ThongTinWeb");

            migrationBuilder.DropColumn(
                name: "PhoneLienHe",
                table: "ThongTinWeb");

            migrationBuilder.DropColumn(
                name: "TenTruong",
                table: "ThongTinWeb");
        }
    }
}
