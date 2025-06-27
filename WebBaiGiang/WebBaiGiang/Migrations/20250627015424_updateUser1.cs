using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebBaiGiang.Migrations
{
    /// <inheritdoc />
    public partial class updateUser1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Password",
                table: "NguoiDung",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "NguoiDung",
                type: "nvarchar(200)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "NguoiDung",
                type: "nvarchar(200)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)");

            migrationBuilder.AlterColumn<string>(
                name: "Avatar",
                table: "NguoiDung",
                type: "nvarchar(500)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Phone",
                table: "NguoiDung",
                type: "nvarchar(40)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(40)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Gender",
                table: "NguoiDung",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Role",
                table: "NguoiDung",
                type: "nvarchar(40)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(40)",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Role",
                table: "NguoiDung",
                type: "nvarchar(40)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(40)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Gender",
                table: "NguoiDung",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Phone",
                table: "NguoiDung",
                type: "nvarchar(40)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(40)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Avatar",
                table: "NguoiDung",
                type: "nvarchar(500)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "NguoiDung",
                type: "nvarchar(200)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "NguoiDung",
                type: "nvarchar(200)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Password",
                table: "NguoiDung",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
